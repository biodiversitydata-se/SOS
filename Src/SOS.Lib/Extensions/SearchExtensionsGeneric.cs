using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace SOS.Lib.Extensions;

/// <summary>
/// ElasticSearch query extensions
/// </summary>
public static class SearchExtensionsGeneric
{
    extension(AggregationSortOrder sortOrder)
    {
        public ICollection<KeyValuePair<Field, SortOrder>> GetTermsOrder()
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

    extension(ElasticsearchClient client)
    {
        /// <summary>
        /// Generates a collection of sort descriptor actions for use with Elasticsearch queries based on the specified
        /// sorting filters.
        /// </summary>
        /// <remarks>If the provided sorting filters do not include the default sort property for the
        /// entity type, it is automatically appended to ensure unique ordering. Resolved property paths are cached per
        /// entity type.</remarks>
        /// <typeparam name="TQueryDescriptor">The type of the query descriptor used to build sort options.</typeparam>
        /// <typeparam name="TEntity">The entity type whose properties are used for sorting.</typeparam>
        /// <param name="indexName">The name of the Elasticsearch index to which the sort descriptors will be applied.</param>
        /// <param name="sortings">A collection of sorting filters that define the fields and order to sort by. If null or empty, no sort
        /// descriptors are generated.</param>
        /// <returns>A collection of actions that configure sort options for the specified query descriptor, or null if no
        /// sorting filters are provided.</returns>
        public Task<ICollection<Action<SortOptionsDescriptor<TQueryDescriptor>>>> GetSortDescriptorAsync<TQueryDescriptor, TEntity>(string indexName, IEnumerable<SortOrderFilter> sortings) where TQueryDescriptor : class where TEntity : class
        {
            var adjustedSortings = sortings?.ToList();
            if (adjustedSortings == null || adjustedSortings.Count == 0)
            {
                return Task.FromResult<ICollection<Action<SortOptionsDescriptor<TQueryDescriptor>>>>(null);
            }

            // make sure that the ordering will be unique.
            string defaultSortProperty = GetDefaultSortPropertyName(typeof(TEntity));
            if (!string.IsNullOrEmpty(defaultSortProperty) &&
                !adjustedSortings.Any(s => string.Equals(s.SortBy?.Trim(), defaultSortProperty, StringComparison.OrdinalIgnoreCase)))
            {
                adjustedSortings.Add(new SortOrderFilter
                {
                    SortBy = defaultSortProperty,
                    SortOrder = SearchSortOrder.Asc
                });
            }

            var sortDescriptor = new List<Action<SortOptionsDescriptor<TQueryDescriptor>>>();
            foreach (var sorting in adjustedSortings)
            {
                var sortBy = ElasticsearchFieldNameHelper.GetFieldName<TEntity>(sorting.SortBy);
                var sortOrder = sorting.SortOrder;
                sortDescriptor.Add(s => s
                    .Field(sortBy, c => c
                        .Order(sortOrder == SearchSortOrder.Desc ? SortOrder.Desc : SortOrder.Asc)
                        .Missing(sortOrder == SearchSortOrder.Desc ? "_last" : "_first")
                    )
                );
            }

            return Task.FromResult<ICollection<Action<SortOptionsDescriptor<TQueryDescriptor>>>>(sortDescriptor);
        }
    }

    extension<TQueryDescriptor>(ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries) where TQueryDescriptor : class
    {
        /// <summary>
        /// Try add date filter criteria
        /// </summary>        
        /// <param name="eventField"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public void TryAddEventDateCritera(
            string eventField, DateFilter filter)
        {
            if (filter != null)
            {
                queries.TryAddDateRangeFilters(filter, $"{eventField}.startDate", $"{eventField}.endDate");
                queries.TryAddTimeRangeFilters(filter, $"{eventField}.startDate");

            }
        }


        /// <summary>
        /// Add date range query to filter
        /// </summary>        
        /// <param name="filter"></param>
        /// <param name="startDateField"></param>
        /// <param name="endDateField"></param>
        public void TryAddDateRangeFilters(DateFilter filter, string startDateField, string endDateField)
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
                    if (filter.StartDate.HasValue || filter.EndDate.HasValue)
                    {
                        queries.Add(q => q.Range(new DateRangeQuery(startDateField)
                        {
                            Gte = filter.StartDate?.ToUniversalTime(),
                            Lte = filter.EndDate?.ToUniversalTime()
                        }));
                    }
                }
                else if (filter.DateFilterType == DateFilter.DateRangeFilterType.OnlyEndDate)
                {
                    if (filter.StartDate.HasValue || filter.EndDate.HasValue)
                    {
                        queries.Add(q => q.Range(new DateRangeQuery(endDateField)
                        {
                            Gte = filter.StartDate?.ToUniversalTime(),
                            Lte = filter.EndDate?.ToUniversalTime()
                        }));
                    }
                }
            }
        }

        public void TryAddGeneralizationsCriteria(
    bool? includeSensitiveGeneralizedObservations, bool? isGeneralized)
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

        }

        /// <summary>
        /// Add time range filters
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        public void TryAddTimeRangeFilters(DateFilter filter, string field)
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
        }
    }
}
