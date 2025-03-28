using MongoDB.Driver;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Elastic.Clients.Elasticsearch.IndexManagement;


namespace SOS.Lib.Extensions
{
    /// <summary>
    /// ElasticSearch query extensions
    /// </summary>
    public static class SearchExtensionsGeneric
    {
        private static ConcurrentDictionary<string, IReadOnlyDictionary<IndexName, TypeFieldMappings>> indicesByKey = new ConcurrentDictionary<string, IReadOnlyDictionary<IndexName, TypeFieldMappings>>();


        public static ICollection<KeyValuePair<Field, SortOrder>> GetTermsOrder(this AggregationSortOrder sortOrder)
        {
            return new List<KeyValuePair<Field, SortOrder>> { 
                sortOrder switch
                {
                    AggregationSortOrder.CountAscending => new KeyValuePair<Field, SortOrder>(Field.CountField, SortOrder.Asc),
                    AggregationSortOrder.KeyAscending => new KeyValuePair<Field, SortOrder>(Field.KeyField, SortOrder.Asc),
                    AggregationSortOrder.KeyDescending => new KeyValuePair<Field, SortOrder>(Field.KeyField, SortOrder.Desc),
                    AggregationSortOrder.OrganismQuantityAscending => new KeyValuePair<Field, SortOrder>(Field.FromString("totalOrganismQuantity"), SortOrder.Asc),
                    AggregationSortOrder.OrganismQuantityDescending => new KeyValuePair<Field, SortOrder>(Field.FromString("totalOrganismQuantity"), SortOrder.Desc),
                    _ => new KeyValuePair<Field, SortOrder>(Field.CountField, SortOrder.Desc)
                }
            };
        }
        
        private static string GetDefaultSortPropertyName(Type type)
        {
            return type switch
            {
                Type t when t == typeof(Dataset) => "identifier",
                Type t when t == typeof(Event) => "eventId",
                Type t when t == typeof(Observation) => "occurrence.occurrenceId",
                _ => null
            };
        }

        /// <summary>
        /// Get sort descriptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="sortings"></param>
        /// <returns></returns>
        public static async Task<ICollection<Action<SortOptionsDescriptor<T>>>> GetSortDescriptorAsync<T>(this ElasticsearchClient client, string indexName, IEnumerable<SortOrderFilter> sortings) where T : class
        {
            if (!sortings?.Any() ?? true)
            {
                return null;
            }

            // make sure that the ordering will be unique.
            var adjustedSortings = sortings.ToList();
            string defaultSortProperty = GetDefaultSortPropertyName(typeof(T));
            if (!string.IsNullOrEmpty(defaultSortProperty) && !sortings.Select(m => m.SortBy?.ToLower().Trim()).Contains(defaultSortProperty))
            {
                adjustedSortings.Add(new SortOrderFilter
                {
                    SortBy = defaultSortProperty,
                    SortOrder = SearchSortOrder.Asc
                });
            }

            var sortDescriptor = new List<Action<SortOptionsDescriptor<T>>>();
            foreach (var sorting in adjustedSortings)
            {
                var sortBy = sorting.SortBy;
                var sortOrder = sorting.SortOrder;

                // Split sort string 
                var propertyNames = sortBy.Split('.');
                // Create a object of current class
                var parent = Activator.CreateInstance(typeof(T));
                var targetProperty = (PropertyInfo)null;

                // Loop throw all levels in passed sort string
                for (var i = 0; i < propertyNames.Length; i++)
                {
                    // Get property info for current property
                    targetProperty = parent?.GetProperty(propertyNames[i]);

                    // Update property name to make sure it's in the correct case
                    if (targetProperty != null)
                    {
                        propertyNames[i] = targetProperty.Name.ToCamelCase();
                    }

                    // As long it's not the last property, it must be a sub object. Create a instance of it since it's the new parent
                    if (i != propertyNames.Length - 1)
                    {
                        parent = Activator.CreateInstance(targetProperty.GetPropertyType());
                    }
                }

                // Target property found, get it's type
                var propertyType = targetProperty?.GetPropertyType();

                // If it's a string, add keyword in order to make the sorting work
                if (propertyType == typeof(string))
                {
                    // GetFieldMappingAsync is case sensitive on field names, use updated property names to avoid errors
                    sortBy = string.Join('.', propertyNames);
                    string key = $"{indexName}-{sortBy}";
                    IReadOnlyDictionary<IndexName, TypeFieldMappings> fieldMappings = null;
                    if (!indicesByKey.TryGetValue(key, out fieldMappings))
                    {
                        var response = await client.Indices.GetFieldMappingAsync(new GetFieldMappingRequest(indexName, sortBy));                        
                        if (response.IsValidResponse)
                        {
                            fieldMappings = response.FieldMappings;
                            indicesByKey.TryAdd(key, fieldMappings);
                        }
                    }
                    /*
                    if (fieldMappings?.Values != null)
                    {       
                        var x = fieldMappings?.Values.Where(tfm => tfm
                            .Mappings.Keys.Where(k => k.Name.Equals(sortBy) && k.Property.)
                        )

                        var hasKeyword = indices
                            .FirstOrDefault().Value.Mappings
                            .FirstOrDefault().Value?.Mapping?.Values?
                            .Select(s => s as TextProperty)?
                            .Where(p => p?.Fields?.ContainsKey("keyword") ?? false)?
                            .Any() ?? false;
                        if (hasKeyword)
                        {
                            sortBy = $"{sortBy}.keyword";
                        }
                    }*/
                }
                sortDescriptor.Add(s => s
                    .Field(sortBy.ToField(), c => c
                        .Order(sortOrder == SearchSortOrder.Desc ? SortOrder.Desc : SortOrder.Asc)
                        .Missing(sortOrder == SearchSortOrder.Desc ? "_last" : "_first")
                    )
                );
            }

            return sortDescriptor;
        }


        /// <summary>
        /// Try add date filter criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="queries"></param>
        /// <param name="eventField"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddEventDateCritera<TQueryDescriptor>(
           this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string eventField, DateFilter filter) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                queries.TryAddDateRangeFilters(filter, $"{eventField}.startDate", $"{eventField}.endDate");
                queries.TryAddTimeRangeFilters(filter, $"{eventField}.startDate");
                
            }
            return queries;
        }


        /// <summary>
        /// Add date range query to filter
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        /// <param name="startDateField"></param>
        /// <param name="endDateField"></param>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddDateRangeFilters<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, DateFilter filter, string startDateField, string endDateField) where TQueryDescriptor : class
        {
            if (filter != null)
            {
                if (filter.DateFilterType == DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate)
                {
                    queries.TryAddDateRangeCriteria(startDateField, filter.StartDate, RangeTypes.GreaterThanOrEquals);
                    queries.TryAddDateRangeCriteria(endDateField, filter.EndDate, RangeTypes.LessThanOrEquals);
                }
                else if (filter.DateFilterType == DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate)
                {
                    queries.TryAddDateRangeCriteria(startDateField, filter.EndDate, RangeTypes.LessThanOrEquals);
                    queries.TryAddDateRangeCriteria(endDateField, filter.StartDate, RangeTypes.GreaterThanOrEquals);

                }
                else if (filter.DateFilterType == DateFilter.DateRangeFilterType.OnlyStartDate)
                {
                    if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                    {
                        queries.TryAddDateRangeCriteria(startDateField, filter.StartDate, RangeTypes.GreaterThanOrEquals);
                        queries.TryAddDateRangeCriteria(startDateField, filter.EndDate, RangeTypes.LessThanOrEquals);
                    }
                }
                else if (filter.DateFilterType == DateFilter.DateRangeFilterType.OnlyEndDate)
                {
                    if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                    {
                        queries.TryAddDateRangeCriteria(endDateField, filter.StartDate, RangeTypes.GreaterThanOrEquals);
                        queries.TryAddDateRangeCriteria(endDateField, filter.EndDate, RangeTypes.LessThanOrEquals);
                    }
                }
            }

            return queries;
        }

        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddGeneralizationsCriteria<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, bool? includeSensitiveGeneralizedObservations, bool? isGeneralized) where TQueryDescriptor : class
        {
            if (includeSensitiveGeneralizedObservations.HasValue)
            {
                queries.Add(q => q 
                    .Bool(p => p
                        .Should(s => s
                            .Term(t => t
                                .Field("hasGeneralizedObservationInOtherIndex")
                                .Value(FieldValue.Boolean(includeSensitiveGeneralizedObservations.Value))),
                            s => s
                            .Bool(t => t
                                .MustNot(s => s
                                    .Exists(e => 
                                    e.Field("hasGeneralizedObservationInOtherIndex")                                
                                    )
                                    )
                                )
                            )
                        )
                    );
            }

            if (isGeneralized.HasValue)
            {
                queries.Add(q => q
                    .Bool(p => p
                        .Should(s => s
                            .Term(t => t
                                .Field("isGeneralized")
                                .Value(FieldValue.Boolean(isGeneralized.Value))),
                            s => s
                            .Bool(t => t
                                .MustNot(s => s
                                    .Exists(e =>
                                        e.Field("isGeneralized")
                                    )
                                )
                            )
                        )
                    )
                );
            }
            return queries;
        }

        /// <summary>
        /// Add time range filters
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="queries"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddTimeRangeFilters<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, DateFilter filter, string field) where TQueryDescriptor : class
        {
            if (filter?.TimeRanges?.Any() ?? false)
            {
                var timeRangeContainers = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
                foreach (var timeRange in filter.TimeRanges)
                {
                    timeRangeContainers.TryAddScript(timeRange switch
                        {
                            DateFilter.TimeRange.Morning => $@"[4, 5, 6, 7, 8].contains(doc['{field}'].value.getHour())",
                            DateFilter.TimeRange.Forenoon => $@"[9, 10, 11, 12].contains(doc['{field}'].value.getHour())",
                            DateFilter.TimeRange.Afternoon => $@"[13, 14, 15, 16, 17].contains(doc['{field}'].value.getHour())",
                            DateFilter.TimeRange.Evening => $@"[18, 19, 20, 21, 22].contains(doc['{field}'].value.getHour())",
                            _ => $@"[23, 0, 1, 2, 3].contains(doc['{field}'].value.getHour())"
                        });
                }

                queries.Add(q => q
                    .Bool(b => b
                        .Should(timeRangeContainers.ToArray())
                    )
                );  
            }
            return queries;
           
        }
    }
}