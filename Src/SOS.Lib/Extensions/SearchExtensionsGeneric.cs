using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
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

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// ElasticSearch query extensions
    /// </summary>
    public static class SearchExtensionsGeneric
    {
        private static ConcurrentDictionary<string, IReadOnlyDictionary<IndexName, TypeFieldMappings>> indicesByKey = new ConcurrentDictionary<string, IReadOnlyDictionary<IndexName, TypeFieldMappings>>();

        /// <summary>
        /// Range types
        /// </summary>
        public enum RangeTypes
        {
            /// <summary>
            /// Greater than value
            /// </summary>
            GreaterThan,
            /// <summary>
            /// Greater than or equal to value
            /// </summary>
            GreaterThanOrEquals,
            /// <summary>
            /// Less than value
            /// </summary>
            LessThan,
            /// <summary>
            /// Less or equal to value
            /// </summary>
            LessThanOrEquals
        }

        /// <summary>
        /// Add geo distance criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="geometry"></param>
        /// <param name="distanceType"></param>
        /// <param name="distance"></param>
        public static void AddGeoDistanceCriteria<TQueryDescriptor>(this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryDescriptor>> query, string field, IGeoShape geometry, GeoDistanceType distanceType, double distance) where TQueryDescriptor : class
        {
          
            query.Add(q => q
                .GeoDistance(gd => gd
                    .Field(field)
                    .DistanceType(distanceType)
                    .Location(geometry.ToGeoLocation())
                    .Distance(distance, DistanceUnit.Meters)
                    .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                )
            );
        }

        /// <summary>
        ///  Add geo shape criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="geometry"></param>
        /// <param name="relation"></param>
        public static void AddGeoShapeCriteria<TQueryDescriptor>(this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryDescriptor>> query, string field, IGeoShape geometry, GeoShapeRelation relation) where TQueryDescriptor : class
        {
            query.Add(q => q
                .GeoShape(gd => gd
                    .Field(field)
                    .Shape(s => geometry)
                    .Relation(relation)
                )
            );
        }

        /// <summary>
        /// Add field must exists criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        public static void AddMustExistsCriteria<TQueryDescriptor>(
            this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryDescriptor>> query, string field) where TQueryDescriptor : class
        {
            query.Add(q => q.Regexp(re => re
                .Field(new Field(field))
                .Value(".+")
            );
            
        }

        /// <summary>
        /// Add nested must exists criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="nestedPath"></param>
        public static void AddNestedMustExistsCriteria<TQueryDescriptor>(
            this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryDescriptor>> query, string nestedPath) where TQueryDescriptor : class
        {
            query.Add(q => q
                .Nested(n => n
                    .Path(nestedPath)
                    .Query(nq => nq
                        .Bool(b => b
                            .Filter(f => f
                                .Exists(e => e
                                    .Field(nestedPath)
                                )
                            )
                        )
                    )
                )
            );
        }

        /// <summary>
        /// Add field not exists criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        public static void AddNotExistsCriteria<TQueryDescriptor>(
            this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryDescriptor>> query, string field) where TQueryDescriptor : class
        {
            query.Add(q => q
                .Bool(b => b
                    .MustNot(mn => mn
                        .Exists(e => e
                            .Field(field)
                        )
                    )
                )
            );
        }

        // Get observations from other than Artportalen too

        /// <summary>
        ///  Add numeric filter with relation operator
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="queryInternal"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="relationalOperator"></param>
        public static void AddNumericFilterWithRelationalOperator<TQueryDescriptor>(this
            ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryDescriptor>> queryInternal, string fieldName,
            int value, string relationalOperator) where TQueryDescriptor : class
        {
            switch (relationalOperator.ToLower())
            {
                case "eq":
                    queryInternal.Add(q => q
                        .Term(r => r
                            .Field(fieldName)
                            .Value(value)
                        )
                    );
                    break;
                case "gte":
                    queryInternal.Add(q => q
                        .Range(r => r
                            .Field(fieldName)
                            .GreaterThanOrEquals(value)
                        )
                    );
                    break;
                case "lte":
                    queryInternal.Add(q => q
                        .Range(r => r
                            .Field(fieldName)
                            .LessThanOrEquals(value)
                        )
                    );
                    break;
            }
        }

        /// <summary>
        ///  Add script source
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="source"></param>
        public static void AddScript<TQueryDescriptor>(this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryContainer>> query, string source) where TQueryDescriptor : class
        {
            query.Add(q => q
                .Script(s => s
                    .Script(sc => sc
                        .Source(source)
                    )
                )
            );
        }

        public static Func<TermsOrderDescriptor<T>, IPromise<IList<TermsOrder>>> GetTermsOrder<T>(this AggregationSortOrder sortOrder) where T : class
        {
            switch (sortOrder)
            {
                case AggregationSortOrder.CountDescending:
                    return m => m.CountDescending();
                case AggregationSortOrder.CountAscending:
                    return m => m.CountAscending();
                case AggregationSortOrder.KeyDescending:
                    return m => m.KeyDescending();
                case AggregationSortOrder.KeyAscending:
                    return m => m.KeyAscending();
                case AggregationSortOrder.OrganismQuantityDescending:
                    return m => m.Descending("totalOrganismQuantity");
                case AggregationSortOrder.OrganismQuantityAscending:
                    return m => m.Ascending("totalOrganismQuantity");
                default:
                    return m => m.CountDescending();
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

        /// <summary>
        /// Get sort descriptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexName"></param>
        /// <param name="sortings"></param>
        /// <returns></returns>
        public static async Task<SortDescriptor<T>> GetSortDescriptorAsync<T>(this IElasticClient client, string indexName, IEnumerable<SortOrderFilter> sortings) where T : class
        {
            if (!sortings?.Any() ?? true)
            {
                return null;
            }

            // make sure that the ordering will be unique.
            var adjustedSortings = sortings.ToList();
            string defaultSortProperty = GetDefaultSortPropertyName(typeof(T));
            if (!defaultSortProperty.IsNullOrEmpty() && !sortings.Select(m => m.SortBy?.ToLower().Trim()).Contains(defaultSortProperty))
            {
                adjustedSortings.Add(new SortOrderFilter
                {
                    SortBy = defaultSortProperty,
                    SortOrder = SearchSortOrder.Asc
                });
            }

            var sortDescriptor = new SortDescriptor<T>();
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
                    IReadOnlyDictionary<IndexName, TypeFieldMappings> indices = null;

                    if (!indicesByKey.TryGetValue(key, out indices))
                    {
                        var response = await client.Indices.GetFieldMappingAsync(new GetFieldMappingRequest(indexName, sortBy));                        
                        if (response.IsValid)
                        {
                            indices = response.Indices;
                            indicesByKey.TryAdd(key, indices);
                        }
                    }              

                    if (indices != null)
                    {                        
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
                    }
                }
                sortDescriptor = sortDescriptor.Field(f => f
                    .Field(sortBy)
                    .Order(sortOrder == SearchSortOrder.Desc ? SortOrder.Descending : SortOrder.Ascending)
                    .Missing(sortOrder == SearchSortOrder.Desc ? "_last" : "_first")
                );
            }

            return sortDescriptor;
        }

        /// <summary>
        /// Cast property to field
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static Field ToField(this string property)
        {
            return new Field(string.Join('.', property.Split('.').Select(p => p
                .ToCamelCase()
            )));
        }

        /// <summary>
        /// Try to add bounding box criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="boundingBox"></param>
        public static void TryAddBoundingBoxCriteria<TQueryDescriptor>(this
            ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryContainer>> query, string field, LatLonBoundingBox boundingBox) where TQueryDescriptor : class
        {
            if (boundingBox?.BottomRight == null || boundingBox?.TopLeft == null)
            {
                return;
            }

            query.Add(q => q
                .GeoBoundingBox(g => g
                    .Field(new Field(field))
                    .BoundingBox(
                        boundingBox.TopLeft.Latitude,
                        boundingBox.TopLeft.Longitude,
                        boundingBox.BottomRight.Latitude,
                        boundingBox.BottomRight.Longitude)
                )
            );
        }

        /// <summary>
        /// Try to add nested term criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="nestedPath"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public static void TryAddNestedTermCriteria<TQueryDescriptor, TValue>(this
            ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryContainer>> query, string nestedPath, string field, TValue value) where TQueryDescriptor : class
        {
            if (value == null)
            {
                return;
            }

            query.Add(q => q
                .Nested(n => n
                    .Path(nestedPath)
                    .Query(q => q
                        .Term(t => t
                            .Field($"{nestedPath}.{field}")
                            .Value(value)
                        )
                    )));
        }

        /// <summary>
        /// Try to add nested terms criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="nestedPath"></param>
        /// <param name="field"></param>
        /// <param name="values"></param>
        public static void TryAddNestedTermsCriteria<TQueryDescriptor, TValue>(this
            ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryContainer>> query, string nestedPath, string field, IEnumerable<TValue> values) where TQueryDescriptor : class
        {
            if (values?.Any() ?? false)
            {
                query.Add(q => q
                    .Nested(n => n
                        .Path(nestedPath)
                        .Query(q => q
                            .Terms(t => t
                                .Field($"{nestedPath}.{field}")
                                .Terms(values)
                            )
                        )));
            }
        }

        /// <summary>
        ///  Add numeric range criteria if value is not null 
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void TryAddNumericRangeCriteria<TQueryDescriptor>(this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryContainer>> query, string field, double? value, RangeTypes type) where TQueryDescriptor : class
        {
            if (value.HasValue)
            {
                switch (type)
                {
                    case RangeTypes.GreaterThan:
                        query.Add(q => q
                            .Range(r => r
                                .Field(field)
                                .GreaterThan(value)
                            )
                        );
                        break;
                    case RangeTypes.GreaterThanOrEquals:
                        query.Add(q => q
                            .Range(r => r
                                .Field(field)
                                .GreaterThanOrEquals(value)
                            )
                        );
                        break;
                    case RangeTypes.LessThan:
                        query.Add(q => q
                            .Range(r => r
                                .Field(field)
                                .LessThan(value)
                            )
                        );
                        break;
                    case RangeTypes.LessThanOrEquals:
                        query.Add(q => q
                            .Range(r => r
                                .Field(field)
                                .LessThanOrEquals(value)
                            )
                        );
                        break;
                }
            }
        }

        /// <summary>
        /// Try add date filter criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="eventField"></param>
        public static void TryAddEventDateCritera<TQueryDescriptor>(
           this QueryDescriptor<TQueryDescriptor> query, DateFilter filter, string eventField) where TQueryDescriptor : class
        {
            if (filter == null)
            {
                return;
            }

            query.TryAddDateRangeFilters(filter, $"{eventField}.startDate", $"{eventField}.endDate");
            query.TryAddTimeRangeFilters(filter, $"{eventField}.startDate");
        }

        /// <summary>
        /// Try add date range criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="dateTime"></param>
        /// <param name="type"></param>
        public static void TryAddDateRangeCriteria<TQueryDescriptor>(this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryContainer>> query, string field, DateTime? dateTime, RangeTypes type) where TQueryDescriptor : class
        {
            if (dateTime.HasValue)
            {
                switch (type)
                {
                    case RangeTypes.GreaterThan:
                        query.Add(q => q
                            .DateRange(r => r
                                .Field(field)
                                .GreaterThan(
                                    DateMath.Anchored(
                                        dateTime.Value.ToUniversalTime()
                                    )
                                )
                            )
                        );
                        break;
                    case RangeTypes.GreaterThanOrEquals:
                        query.Add(q => q
                            .DateRange(r => r
                                .Field(field)
                                .GreaterThanOrEquals(
                                    DateMath.Anchored(
                                        dateTime.Value.ToUniversalTime()
                                    )
                                )
                            )
                        );
                        break;
                    case RangeTypes.LessThan:
                        query.Add(q => q
                            .DateRange(r => r
                                .Field(field)
                                .LessThan(
                                    DateMath.Anchored(
                                        dateTime.Value.ToUniversalTime()
                                    )
                                )
                            )
                        );
                        break;
                    case RangeTypes.LessThanOrEquals:
                        query.Add(q => q
                            .DateRange(r => r
                                .Field(field)
                                .LessThanOrEquals(
                                    DateMath.Anchored(
                                        dateTime.Value.ToUniversalTime()
                                    )
                                )
                            )
                        );
                        break;
                }
            }

        }

        /// <summary>
        /// Add date range query to filter
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="startDateField"></param>
        /// <param name="endDateField"></param>
        public static void TryAddDateRangeFilters<TQueryDescriptor>(this ICollection<Func<QueryDescriptor<TQueryDescriptor>, QueryContainer>> query, DateFilter filter, string startDateField, string endDateField) where TQueryDescriptor : class
        {
            if (filter == null)
            {
                return;
            }

            if (filter.DateFilterType == DateFilter.DateRangeFilterType.BetweenStartDateAndEndDate)
            {
                query.TryAddDateRangeCriteria(startDateField, filter.StartDate, RangeTypes.GreaterThanOrEquals);
                query.TryAddDateRangeCriteria(endDateField, filter.EndDate, RangeTypes.LessThanOrEquals);
            }
            else if (filter.DateFilterType == DateFilter.DateRangeFilterType.OverlappingStartDateAndEndDate)
            {
                query.TryAddDateRangeCriteria(startDateField, filter.EndDate, RangeTypes.LessThanOrEquals);
                query.TryAddDateRangeCriteria(endDateField, filter.StartDate, RangeTypes.GreaterThanOrEquals);

            }
            else if (filter.DateFilterType == DateFilter.DateRangeFilterType.OnlyStartDate)
            {
                if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                {
                    query.TryAddDateRangeCriteria(startDateField, filter.StartDate, RangeTypes.GreaterThanOrEquals);
                    query.TryAddDateRangeCriteria(startDateField, filter.EndDate, RangeTypes.LessThanOrEquals);
                }
            }
            else if (filter.DateFilterType == DateFilter.DateRangeFilterType.OnlyEndDate)
            {
                if (filter.StartDate.HasValue && filter.EndDate.HasValue)
                {
                    query.TryAddDateRangeCriteria(endDateField, filter.StartDate, RangeTypes.GreaterThanOrEquals);
                    query.TryAddDateRangeCriteria(endDateField, filter.EndDate, RangeTypes.LessThanOrEquals);
                }
            }
        }

        /// <summary>
        /// Add nested term criteria with and
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nestedPath"></param>
        /// <param name="fieldValues"></param>
        public static void TryAddNestedTermAndCriteria(this
            ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query, string nestedPath,
            IDictionary<string, object> fieldValues)
        {
            if (!fieldValues?.Any() ?? true)
            {
                return;
            }

            var nestedQuery = new List<Func<QueryDescriptor<dynamic>, QueryContainer>>();

            foreach (var fieldValue in fieldValues)
            {
                nestedQuery.TryAddTermCriteria($"{nestedPath}.{fieldValue.Key}", fieldValue.Value);
            }

            query.Add(q => q
                .Nested(n => n
                    .Path(nestedPath)
                    .Query(q => q
                        .Bool(b => b
                            .Must(nestedQuery)
                        )
                    )));
        }

        public static void TryAddAndTermCriteria(this ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query,            
            IDictionary<string, object> fieldValues)
        {
            if (!fieldValues?.Any() ?? true)
            {
                return;
            }

            var objectQuery = new List<Func<QueryDescriptor<dynamic>, QueryContainer>>();

            foreach (var fieldValue in fieldValues)
            {
                objectQuery.TryAddTermCriteria($"{fieldValue.Key}", fieldValue.Value);
            }

            query.Add(q => q
                .Bool(b => b
                    .Must(objectQuery)
                ));
        }

        public static void TryAddAndCriteria(this ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query,
            List<Func<QueryDescriptor<dynamic>, QueryContainer>> queries)
        {
            if (!queries?.Any() ?? true)
            {
                return;
            }
            
            query.Add(q => q
                .Bool(b => b
                    .Must(queries)
                ));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <typeparam name="TTerms"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="terms"></param>
        public static void TryAddTermsCriteria<TQueryDescriptor, TTerms>(
                    this QueryDescriptor<TQueryDescriptor> query, string field, IEnumerable<TTerms> terms) where TQueryDescriptor : class
        {
            if (terms?.Any() ?? false)
            {
                if (terms is IEnumerable<bool> boolValues)
                {
                    query
                        .Terms(t => t
                            .Field(field)
                            .Terms(new TermsQueryField(boolValues.Select(b => FieldValue.Boolean(b)).ToArray()))
                        );
                    return;
                }
                if (terms is IEnumerable<double> doubleValues)
                {
                    query
                        .Terms(t => t
                            .Field(field)
                            .Terms(new TermsQueryField(doubleValues.Select(d => FieldValue.Double(d)).ToArray()))
                        );
                    return;
                }
                if (terms is IEnumerable<long> longValues)
                {
                    query
                         .Terms(t => t
                             .Field(field)
                             .Terms(new TermsQueryField(longValues.Select(b => FieldValue.Long(b)).ToArray()))
                         );
                    return;
                }
                if (terms is IEnumerable<int> intValues)
                {
                    query
                        .Terms(t => t
                            .Field(field)
                            .Terms(new TermsQueryField(intValues.Select(i => FieldValue.Long(i)).ToArray()))
                        );
                    return;
                }
                if (terms is IEnumerable<short> shortValues)
                {
                    query
                         .Terms(t => t
                             .Field(field)
                             .Terms(new TermsQueryField(shortValues.Select(s => FieldValue.Long(s)).ToArray()))
                         );
                    return;
                }
                if (terms is IEnumerable<byte> byteValues)
                {
                    query
                        .Terms(t => t
                            .Field(field)
                            .Terms(new TermsQueryField(byteValues.Select(b => FieldValue.Long(b)).ToArray()))
                        );
                    return;
                }

                query
                    .Terms(t => t
                        .Field(field)
                        .Terms(new TermsQueryField(terms.Select(s => FieldValue.String(s?.ToString())).ToArray()))
                    );
            }
        }

        public static void AddExistsCriteria<TQueryDescriptor>(
            this QueryDescriptor<TQueryDescriptor> query, string field) where TQueryDescriptor : class
        {
            query.Exists(e => e
                .Field(field)
            );
        }

        /// <summary>
        ///  Try to add query criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public static void TryAddTermCriteria<TQueryDescriptor, TValue>(
            this QueryDescriptor<TQueryDescriptor> query, string field, TValue value) where TQueryDescriptor : class
        {
            if (!string.IsNullOrEmpty(value?.ToString()))
            {
                if (value is bool boolValue)
                {
                    query.Term(m => m.Field(field).Value(FieldValue.Boolean(boolValue)));
                    return;
                }
                if (value is double doubleValue)
                {
                    query.Term(m => m.Field(field).Value(FieldValue.Double(doubleValue)));
                    return;
                }
                if (value is long longValue )
                {
                    query.Term(m => m.Field(field).Value(FieldValue.Long(longValue)));
                    return;
                }
                if (value is int intValue)
                {
                    query.Term(m => m.Field(field).Value(FieldValue.Long(intValue)));
                    return;
                }
                if (value is short shortValue)
                {
                    query.Term(m => m.Field(field).Value(FieldValue.Long(shortValue)));
                    return;
                }
                if (value is byte byteValue)
                {
                    query.Term(m => m.Field(field).Value(FieldValue.Long(byteValue)));
                    return;
                }

                query.Term(m => m.Field(field).Value(FieldValue.String(value?.ToString())));
            }
        }

        
        public static void TryAddGeneralizationsCriteria<TQueryDescriptor>(
            this QueryDescriptor<TQueryDescriptor> query, bool? includeSensitiveGeneralizedObservations, bool? isGeneralized) where TQueryDescriptor : class
        {
            if (includeSensitiveGeneralizedObservations.HasValue)
            {
                query.Bool(p => p
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
                    );
            }

            if (isGeneralized.HasValue)
            {
                query.Bool(p => p
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
                );
            }
        }

        /// <summary>
        /// Try to add query criteria where property must match a specified value
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="matchValue"></param>
        public static void TryAddTermCriteria<TQueryDescriptor, TValue>(
           this QueryDescriptor<TQueryDescriptor> query, string field, TValue value, TValue matchValue) where TQueryDescriptor : class
        {
            if (!string.IsNullOrEmpty(value?.ToString()) && matchValue.Equals(value))
            {
                query.TryAddTermCriteria(field, value);
            }
        }

        /// <summary>
        /// Add time range filters
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        public static void TryAddTimeRangeFilters<TQueryDescriptor>(this QueryDescriptor<TQueryDescriptor> query, DateFilter filter, string field) where TQueryDescriptor : class
        {
            if (!filter?.TimeRanges?.Any() ?? true)
            {
                return;
            }

            var timeRangeContainers = new List<Func<QueryDescriptor<TQueryDescriptor>, QueryDescriptor>>();
            foreach (var timeRange in filter.TimeRanges)
            {
                switch (timeRange)
                {
                    case DateFilter.TimeRange.Morning:
                        timeRangeContainers.AddScript($@"[4, 5, 6, 7, 8].contains(doc['{field}'].value.getHour())");
                        break;
                    case DateFilter.TimeRange.Forenoon:
                        timeRangeContainers.AddScript($@"[9, 10, 11, 12].contains(doc['{field}'].value.getHour())");
                        break;
                    case DateFilter.TimeRange.Afternoon:
                        timeRangeContainers.AddScript($@"[13, 14, 15, 16, 17].contains(doc['{field}'].value.getHour())");
                        break;
                    case DateFilter.TimeRange.Evening:
                        timeRangeContainers.AddScript($@"[18, 19, 20, 21, 22].contains(doc['{field}'].value.getHour())");
                        break;
                    default:
                        timeRangeContainers.AddScript($@"[23, 0, 1, 2, 3].contains(doc['{field}'].value.getHour())");
                        break;
                }
            }

            query.Add(q => q
                .Bool(b => b
                    .Should(timeRangeContainers)
                )
            );
        }

        /// <summary>
        /// Add wildcard criteria
        /// </summary>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="wildcard"></param>
        public static void TryAddWildcardCriteria(this ICollection<Func<QueryDescriptor<dynamic>, QueryContainer>> query, string field, string wildcard)
        {
            if (string.IsNullOrEmpty(wildcard))
            {
                return;
            }

            query.Add(q => q
                .Wildcard(w => w
                    .Field(field)
                    .CaseInsensitive(true)
                    .Value(wildcard)
                )
            );
        }
    }
}