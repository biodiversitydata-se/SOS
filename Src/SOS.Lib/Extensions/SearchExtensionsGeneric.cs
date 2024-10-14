﻿using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using System;
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="geometry"></param>
        /// <param name="distanceType"></param>
        /// <param name="distance"></param>
        public static void AddGeoDistanceCriteria<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field, IGeoShape geometry, GeoDistanceType distanceType, double distance) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="geometry"></param>
        /// <param name="relation"></param>
        public static void AddGeoShapeCriteria<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field, IGeoShape geometry, GeoShapeRelation relation) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        public static void AddMustExistsCriteria<TQueryContainer>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field) where TQueryContainer : class
        {
            query.Add(q => q
                .Regexp(re => re.Field(field).Value(".+"))
            );
        }

        /// <summary>
        /// Add nested must exists criteria
        /// </summary>
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="nestedPath"></param>
        public static void AddNestedMustExistsCriteria<TQueryContainer>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string nestedPath) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        public static void AddNotExistsCriteria<TQueryContainer>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="queryInternal"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="relationalOperator"></param>
        public static void AddNumericFilterWithRelationalOperator<TQueryContainer>(this
            ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> queryInternal, string fieldName,
            int value, string relationalOperator) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="source"></param>
        public static void AddScript<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string source) where TQueryContainer : class
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
                default:
                    return m => m.CountDescending();
            }
        }


        /// <summary>
        /// Get sort descriptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="indexNames"></param>
        /// <param name="sortings"></param>
        /// <returns></returns>
        public static async Task<SortDescriptor<T>> GetSortDescriptorAsync<T>(this IElasticClient client, string indexNames, IEnumerable<SortOrderFilter> sortings) where T : class
        {
            if (!sortings?.Any() ?? true)
            {
                return null;
            }

            var sortDescriptor = new SortDescriptor<T>();
            foreach (var sorting in sortings)
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
                    var response =
                        await client.Indices.GetFieldMappingAsync(new GetFieldMappingRequest(indexNames, sortBy));

                    if (response.IsValid)
                    {
                        var hasKeyword = response.Indices
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="boundingBox"></param>
        public static void TryAddBoundingBoxCriteria<TQueryContainer>(this
            ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field, LatLonBoundingBox boundingBox) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="nestedPath"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public static void TryAddNestedTermCriteria<TQueryContainer, TValue>(this
            ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string nestedPath, string field, TValue value) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="nestedPath"></param>
        /// <param name="field"></param>
        /// <param name="values"></param>
        public static void TryAddNestedTermsCriteria<TQueryContainer, TValue>(this
            ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string nestedPath, string field, IEnumerable<TValue> values) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static void TryAddNumericRangeCriteria<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field, double? value, RangeTypes type) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="eventField"></param>
        public static void TryAddEventDateCritera<TQueryContainer>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, DateFilter filter, string eventField) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="dateTime"></param>
        /// <param name="type"></param>
        public static void TryAddDateRangeCriteria<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field, DateTime? dateTime, RangeTypes type) where TQueryContainer : class
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
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="startDateField"></param>
        /// <param name="endDateField"></param>
        public static void TryAddDateRangeFilters<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, DateFilter filter, string startDateField, string endDateField) where TQueryContainer : class
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
            ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string nestedPath,
            IDictionary<string, object> fieldValues)
        {
            if (!fieldValues?.Any() ?? true)
            {
                return;
            }

            var nestedQuery = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

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

        /// <summary>
        /// Try to add query criteria
        /// </summary>
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <typeparam name="TTerms"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="terms"></param>
        public static void TryAddTermsCriteria<TQueryContainer, TTerms>(
                    this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field, IEnumerable<TTerms> terms) where TQueryContainer : class
        {
            if (terms?.Any() ?? false)
            {
                query.Add(q => q
                    .Terms(t => t
                        .Field(field)
                        .Terms(terms)
                    )
                );
            }
        }

        public static void AddExistsCriteria<TQueryContainer>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field) where TQueryContainer : class
        {
            query.Add(q => q
                .Exists(e => e
                    .Field(field)
                )
            );
        }

        /// <summary>
        /// Try to add query criteria
        /// </summary>
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        public static void TryAddTermCriteria<TQueryContainer, TValue>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field, TValue value) where TQueryContainer : class
        {
            if (!string.IsNullOrEmpty(value?.ToString()))
            {
                query.Add(q => q
                    .Term(m => m.Field(field).Value(value))); // new Field(field)
            }
        }

        public static void TryAddGeneralizationsCriteria<TQueryContainer>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, bool? includeSensitiveGeneralizedObservations, bool? isGeneralized) where TQueryContainer : class
        {
            if (includeSensitiveGeneralizedObservations.HasValue)
            {
                query.Add(q => q
                    .Bool(p => p
                        .Should(s => s
                            .Term(t => t
                                .Field("hasGeneralizedObservationInOtherIndex")
                                .Value(includeSensitiveGeneralizedObservations)),
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
                query.Add(q => q
                    .Bool(p => p
                        .Should(s => s
                            .Term(t => t
                                .Field("isGeneralized")
                                .Value(isGeneralized)),
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
        /// Try to add query criteria where property must match a specified value
        /// </summary>
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="matchValue"></param>
        public static void TryAddTermCriteria<TQueryContainer, TValue>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, string field, TValue value, TValue matchValue) where TQueryContainer : class
        {
            if (!string.IsNullOrEmpty(value?.ToString()) && matchValue.Equals(value))
            {
                query.Add(q => q
                    .Term(m => m.Field(field).Value(value)));
            }
        }

        /// <summary>
        /// Add time range filters
        /// </summary>
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="field"></param>
        public static void TryAddTimeRangeFilters<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query, DateFilter filter, string field) where TQueryContainer : class
        {
            if (!filter?.TimeRanges?.Any() ?? true)
            {
                return;
            }

            var timeRangeContainers = new List<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>>();
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
        public static void TryAddWildcardCriteria(this ICollection<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> query, string field, string wildcard)
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