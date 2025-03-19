using MongoDB.Driver;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using SOS.Lib.Models.Gis;
using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Clients.Elasticsearch.Core.Search;
using NetTopologySuite.Geometries;

namespace SOS.Lib.Extensions
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
    /// ElasticSearch query extensions
    /// </summary>
    public static class ElasticSearchGenericSearchExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchRequestDescriptor"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static SearchRequestDescriptor<T> AddDefaultAggrigationSettings<T>(this SearchRequestDescriptor<T> searchRequestDescriptor, int? size = 0)
        {
            return searchRequestDescriptor
                .Size(size)
                .Source(new SourceConfig(new SourceFilter { Excludes = new[] { "*" }.ToFields() }))
                .TrackTotalHits(new TrackHits(false));
        }

        /// <summary>
        /// Add geo distance criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="point"></param>
        /// <param name="distanceType"></param>
        /// <param name="distance"></param>
        public static QueryDescriptor<TQueryDescriptor> AddGeoDistanceCriteria<TQueryDescriptor>(this QueryDescriptor<TQueryDescriptor> query, string field, Point point, GeoDistanceType distanceType, double distance) where TQueryDescriptor : class
        {
            return query.GeoDistance(gd => gd
                .Distance($"{(int)distance}m")
                .DistanceType(distanceType)
                .Field(field)
                .Location(point.ToGeoLocation())
                .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
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
        public static QueryDescriptor<TQueryDescriptor> AddGeoShapeCriteria<TQueryDescriptor>(this QueryDescriptor<TQueryDescriptor> query, string field, Geometry geometry, GeoShapeRelation relation) where TQueryDescriptor : class
        {
            return query.GeoShape(gd => gd
                .Field(field.ToField())
                .Shape(s => s
                    .Shape(geometry.ToGeoJson())
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
        public static QueryDescriptor<TQueryDescriptor> AddMustExistsCriteria<TQueryDescriptor>(
            this QueryDescriptor<TQueryDescriptor> query, string field) where TQueryDescriptor : class
        {
            return query.Regexp(re => re
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
        public static QueryDescriptor<TQueryDescriptor> AddNestedMustExistsCriteria<TQueryDescriptor>(
            this QueryDescriptor<TQueryDescriptor> query, string nestedPath) where TQueryDescriptor : class
        {
            return query.Nested(n => n
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
            );
        }

        /// <summary>
        /// Add field not exists criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        public static QueryDescriptor<TQueryDescriptor> AddNotExistsCriteria<TQueryDescriptor>(
            this QueryDescriptor<TQueryDescriptor> query, string field) where TQueryDescriptor : class
        {
            return query.Bool(b => b
                .MustNot(mn => mn
                    .Exists(e => e
                        .Field(field)
                    )
                )
            );
        }

        /// <summary>
        ///  Add numeric filter with relation operator
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="relationalOperator"></param>
        public static QueryDescriptor<TQueryDescriptor> AddNumericFilterWithRelationalOperator<TQueryDescriptor>(
            this QueryDescriptor<TQueryDescriptor> query, 
            string fieldName,
            int value, string relationalOperator) where TQueryDescriptor : class
        {
            return relationalOperator.ToLower() switch
            {
                "eq" => query.Term(r => r
                        .Field(fieldName)
                        .Value(value)
                    ),
                "gte" => query.Range(r => r
                        .NumberRange(nr => nr
                            .Field(fieldName)
                            .Gte(value)
                        )
                    ),
                "lt" => query.Range(r => r
                        .NumberRange(nr => nr
                            .Field(fieldName)
                            .Lt(value)
                        )
                    ),
                "lte" => query.Range(r => r
                        .NumberRange(nr => nr
                            .Field(fieldName)
                            .Lte(value)
                        )
                    ),
                _ => query
            };
        }

        /// <summary>
        ///  Add script source
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="source"></param>
        public static QueryDescriptor<TQueryDescriptor> TryAddScript<TQueryDescriptor>(this QueryDescriptor<TQueryDescriptor> query, string source) where TQueryDescriptor : class
        {
            if (string.IsNullOrEmpty(source))
            {
                return query;
            }

            return query.Script(s => s
                .Script(sc => sc
                    .Source(source)
                )
            );
        }

        /// <summary>
        /// Try to add bounding box criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="boundingBox"></param>
        public static QueryDescriptor<TQueryDescriptor> TryAddBoundingBoxCriteria<TQueryDescriptor>(
            this QueryDescriptor<TQueryDescriptor> query, 
            string field, 
            LatLonBoundingBox boundingBox) where TQueryDescriptor : class
        {
            if (boundingBox?.BottomRight == null || boundingBox?.TopLeft == null)
            {
                return query;
            }

            return query.GeoBoundingBox(g => g
                .Field(new Field(field))
                .BoundingBox(
                    GeoBounds.Coordinates(new CoordsGeoBounds
                    {
                        Bottom = boundingBox.TopLeft.Latitude,
                        Left = boundingBox.TopLeft.Longitude,
                        Right = boundingBox.BottomRight.Longitude,
                        Top = boundingBox.BottomRight.Latitude
                    })
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
        public static QueryDescriptor<TQueryDescriptor> TryAddNestedTermCriteria<TQueryDescriptor, TValue>(
            this QueryDescriptor<TQueryDescriptor> query, 
            string nestedPath, string field, TValue value) where TQueryDescriptor : class
        {
            if (value == null)
            {
                return query;
            }

            return query.Nested(n => n
                .Path(nestedPath)
                .Query(q => q
                    .Term(t => t
                        .Field($"{nestedPath}.{field}")
                        .Value(value.ToFieldValue())
                    )
                )
            );
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
        public static QueryDescriptor<TQueryDescriptor> TryAddNestedTermsCriteria<TQueryDescriptor, TValue>(
            this QueryDescriptor<TQueryDescriptor> query, string nestedPath, string field, IEnumerable<TValue> values) where TQueryDescriptor : class
        {
            if (values?.Any() ?? false)
            {
                return query.Nested(n => n
                    .Path(nestedPath)
                    .Query(q => q
                        .Terms(t => t
                            .Field($"{nestedPath}.{field}")
                            .Terms(new TermsQueryField(values.ToFieldValues()))
                        )
                    )
                );
            }

            return query;
        }

        /// <summary>
        ///  Add numeric range criteria if value is not null 
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public static QueryDescriptor<TQueryDescriptor> TryAddNumericRangeCriteria<TQueryDescriptor>(this QueryDescriptor<TQueryDescriptor> query, string field, double? value, RangeTypes type) where TQueryDescriptor : class
        {
            return type switch
            {
                RangeTypes.GreaterThan =>
                        query.Range(r => r
                            .NumberRange(nr => nr
                                .Field(field.ToField())
                                .Gt(value)
                            )
                        ),
                RangeTypes.GreaterThanOrEquals =>
                        query.Range(r => r
                            .NumberRange(nr => nr
                                .Field(field.ToField())
                                .Gte(value)
                            )
                        ),
                RangeTypes.LessThan =>
                        query.Range(r => r
                            .NumberRange(nr => nr
                                .Field(field.ToField())
                                .Lt(value)
                            )
                        ),
                RangeTypes.LessThanOrEquals =>
                        query.Range(r => r
                            .NumberRange(nr => nr
                                .Field(field.ToField())
                                .Lte(value)
                            )
                        ),
                _ => query
            };
        }

        /// <summary>
        /// Try add date range criteria
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="dateTime"></param>
        /// <param name="type"></param>
        public static QueryDescriptor<TQueryDescriptor> TryAddDateRangeCriteria<TQueryDescriptor>(this QueryDescriptor<TQueryDescriptor> query, string field, DateTime? dateTime, RangeTypes type) where TQueryDescriptor : class
        {
            if (!dateTime.HasValue)
            {
                return query;
            }
            
            return type switch
            {
                RangeTypes.GreaterThan =>
                    query.Range(r => r
                        .DateRange(dr => dr
                            .Field(field.ToField())
                            .Gt(DateMath.Anchored(dateTime.Value.ToUniversalTime()))
                        )
                    ),
                RangeTypes.GreaterThanOrEquals =>
                    query.Range(r => r
                        .DateRange(dr => dr
                            .Field(field.ToField())
                            .Gte(DateMath.Anchored(dateTime.Value.ToUniversalTime()))
                        )
                    ),
                RangeTypes.LessThan =>
                    query.Range(r => r
                        .DateRange(dr => dr
                            .Field(field.ToField())
                            .Lt(DateMath.Anchored(dateTime.Value.ToUniversalTime()))
                        )
                    ),
                RangeTypes.LessThanOrEquals =>
                    query.Range(r => r
                        .DateRange(dr => dr
                            .Field(field.ToField())
                            .Lte(DateMath.Anchored(dateTime.Value.ToUniversalTime()))
                        )
                    ),
                _ => query
            };
        }

        /// <summary>
        /// Add nested term criteria with and
        /// </summary>
        /// <param name="query"></param>
        /// <param name="nestedPath"></param>
        /// <param name="fieldValues"></param>
        public static QueryDescriptor<TQueryDescriptor> TryAddNestedTermAndCriteria<TQueryDescriptor>(this
            QueryDescriptor<TQueryDescriptor> query, string nestedPath,
            IDictionary<string, object> fieldValues) where TQueryDescriptor : class
        {
            if (!fieldValues?.Any() ?? true)
            {
                return query;
            }

            var nestedQuery = new QueryDescriptor<TQueryDescriptor>();

            foreach (var fieldValue in fieldValues)
            {
                nestedQuery.TryAddTermCriteria($"{nestedPath}.{fieldValue.Key}", fieldValue.Value);
            }

            return query.Nested(n => n
                .Path(nestedPath)
                .Query(q => q
                    .Bool(b => b
                        .Must(nestedQuery)
                    )
                )
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <typeparam name="TTerms"></typeparam>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="terms"></param>
        public static QueryDescriptor<TQueryDescriptor> TryAddTermsCriteria<TQueryDescriptor, TTerms>(
                    this QueryDescriptor<TQueryDescriptor> query, string field, IEnumerable<TTerms> terms) where TQueryDescriptor : class
        {
            if (terms?.Any() ?? false)
            {
                if (terms is IEnumerable<bool> boolValues)
                {
                    return query
                        .Terms(t => t
                            .Field(field)
                            .Terms(new TermsQueryField(boolValues.Select(b => FieldValue.Boolean(b)).ToArray()))
                        );
                }
                if (terms is IEnumerable<double> doubleValues)
                {
                    return query
                        .Terms(t => t
                            .Field(field)
                            .Terms(new TermsQueryField(doubleValues.Select(d => FieldValue.Double(d)).ToArray()))
                        );
                }
                if (terms is IEnumerable<long> longValues)
                {
                    return query
                         .Terms(t => t
                             .Field(field)
                             .Terms(new TermsQueryField(longValues.Select(b => FieldValue.Long(b)).ToArray()))
                         );
                }
                if (terms is IEnumerable<int> intValues)
                {
                    return query
                        .Terms(t => t
                            .Field(field)
                            .Terms(new TermsQueryField(intValues.Select(i => FieldValue.Long(i)).ToArray()))
                        );
                }
                if (terms is IEnumerable<short> shortValues)
                {
                    return query
                         .Terms(t => t
                             .Field(field)
                             .Terms(new TermsQueryField(shortValues.Select(s => FieldValue.Long(s)).ToArray()))
                         );
                }
                if (terms is IEnumerable<byte> byteValues)
                {
                    return query
                        .Terms(t => t
                            .Field(field)
                            .Terms(new TermsQueryField(byteValues.Select(b => FieldValue.Long(b)).ToArray()))
                        );
                }

                return query
                    .Terms(t => t
                        .Field(field)
                        .Terms(new TermsQueryField(terms.Select(s => FieldValue.String(s?.ToString())).ToArray()))
                    );
            }
            return query;
        }

        public static QueryDescriptor<TQueryDescriptor> AddExistsCriteria<TQueryDescriptor>(
            this QueryDescriptor<TQueryDescriptor> query, string field) where TQueryDescriptor : class
        {
            return query.Exists(e => e
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
        public static QueryDescriptor<TQueryDescriptor> TryAddTermCriteria<TQueryDescriptor, TValue>(
            this QueryDescriptor<TQueryDescriptor> query, string field, TValue value) where TQueryDescriptor : class
        {
            if (string.IsNullOrEmpty(value?.ToString()))
            {
                return query;
            }
            
            if (value is bool boolValue)
            {
                return query.Term(m => m.Field(field).Value(FieldValue.Boolean(boolValue)));
            }
            if (value is double doubleValue)
            {
                return query.Term(m => m.Field(field).Value(FieldValue.Double(doubleValue)));
            }
            if (value is long longValue )
            {
                return query.Term(m => m.Field(field).Value(FieldValue.Long(longValue)));
            }
            if (value is int intValue)
            {
                return query.Term(m => m.Field(field).Value(FieldValue.Long(intValue)));
            }
            if (value is short shortValue)
            {
                return query.Term(m => m.Field(field).Value(FieldValue.Long(shortValue)));
            }
            if (value is byte byteValue)
            {
                return query.Term(m => m.Field(field).Value(FieldValue.Long(byteValue)));
            }

            return query.Term(m => m.Field(field).Value(FieldValue.String(value?.ToString())));
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
        public static QueryDescriptor<TQueryDescriptor> TryAddTermCriteria<TQueryDescriptor, TValue>(
           this QueryDescriptor<TQueryDescriptor> query, string field, TValue value, TValue matchValue) where TQueryDescriptor : class
        {
            if (!string.IsNullOrEmpty(value?.ToString()) && matchValue.Equals(value))
            {
                return query.TryAddTermCriteria(field, value);
            }
            return query;
        }

        /// <summary>
        /// Add wildcard criteria
        /// </summary>
        /// <param name="query"></param>
        /// <param name="field"></param>
        /// <param name="wildcard"></param>
        public static QueryDescriptor<TQueryDescriptor> TryAddWildcardCriteria<TQueryDescriptor>(this QueryDescriptor<TQueryDescriptor> query, string field, string wildcard) where TQueryDescriptor : class
        {
            if (string.IsNullOrEmpty(wildcard))
            {
                return query;
            }

            return query.Wildcard(w => w
                .Field(field)
                .CaseInsensitive(true)
                .Value(wildcard)
            );
        }
    }
}