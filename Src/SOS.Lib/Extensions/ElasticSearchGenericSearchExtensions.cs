using MongoDB.Driver;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using SOS.Lib.Models.Gis;
using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Clients.Elasticsearch.Core.Search;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace SOS.Lib.Extensions;

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
    /// Get default settings for aggregations
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="searchRequestDescriptor"></param>
    /// <param name="size"></param>
    /// <param name="trackHits"></param>
    /// <returns></returns>
    public static SearchRequestDescriptor<T> AddDefaultAggrigationSettings<T>(this SearchRequestDescriptor<T> searchRequestDescriptor, int? size = 0, bool trackHits = false)
    {
        return searchRequestDescriptor
            .Size(size)
            .TrackTotalHits(new TrackHits(trackHits));
    }

    /// <summary>
    /// Add existsing critera
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public static void AddExistsCriteria<TQueryDescriptor>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field) where TQueryDescriptor : class
    {
        queries.Add(q => q
            .Exists(e => e
                .Field(field)
            )
        );
    }

    /// <summary>
    /// Add field must exists criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    public static void AddMustExistsCriteria<TQueryDescriptor>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field) where TQueryDescriptor : class
    {
        queries.Add(q => q
            .Regexp(re => re
                .Field(new Field(field))
                .Value(".+")
            )
        );
    }

    /// <summary>
    /// Add nested must exists criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="nestedPath"></param>
    public static void AddNestedMustExistsCriteria<TQueryDescriptor>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string nestedPath) where TQueryDescriptor : class
    {
        queries.Add(q => q
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
    /// <param name="queries"></param>
    /// <param name="field"></param>
    public static void AddNotExistsCriteria<TQueryDescriptor>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field) where TQueryDescriptor : class
    {
        queries.Add(q => q
            .Bool(b => b
                .MustNot(mn => mn
                    .Exists(e => e
                        .Field(field)
                    )
                )
            )
        );
    }

    public static void TryAddExistsCriteria<TQueryDescriptor>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, bool? exists) where TQueryDescriptor : class
    {
        if (exists.HasValue)
        {
            if (exists.Value == true)
            {
                queries.Add(q => q.Exists(e => e.Field(field)));
            }
            else
            {
                queries.Add(q => q.Bool(b => b.MustNot(mn => mn.Exists(e => e.Field(field)))));
            }                
        }
    }

    /// <summary>
    ///  Add numeric filter with relation operator
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="fieldName"></param>
    /// <param name="value"></param>
    /// <param name="relationalOperator"></param>
    public static void AddNumericFilterWithRelationalOperator<TQueryDescriptor>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, 
        string fieldName,
        int value, string relationalOperator) where TQueryDescriptor : class
    {
        switch (relationalOperator.ToLower())
        {
            case "eq":
                queries.Add(q => q
                    .Term(r => r
                        .Field(fieldName)
                        .Value(value)
                    )
                );
                break;
            case "gte":
                queries.Add(q => q
                    .Range(r => r
                        .NumberRange(nr => nr
                            .Field(fieldName)
                            .Gte(value)
                        )
                    )
                );
                break;
            case "lt":
                queries.Add(q => q
                    .Range(r => r
                        .NumberRange(nr => nr
                            .Field(fieldName)
                            .Lt(value)
                        )
                    )
                );
                break;
            case "lte":
                queries.Add(q => q
                    .Range(r => r
                        .NumberRange(nr => nr
                            .Field(fieldName)
                            .Lte(value)
                        )
                    )
                );
                break;
        }
    }

    /// <summary>
    ///  Add script source
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="source"></param>
    public static void TryAddScript<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string source) where TQueryDescriptor : class
    {
        if (!string.IsNullOrEmpty(source))
        {
            queries.Add(q => q
                 .Script(s => s
                    .Script(sc => sc
                        .Source(source)
                    )
                )
            );
        }
    }

    /// <summary>
    /// Try to add bounding box criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="boundingBox"></param>
    public static void TryAddBoundingBoxCriteria<TQueryDescriptor>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, 
        string field, 
        LatLonBoundingBox boundingBox) where TQueryDescriptor : class
    {
        if (boundingBox?.BottomRight != null || boundingBox?.TopLeft != null)
        {
            queries.Add(q => q
                .GeoBoundingBox(g => g
                    .Field(new Field(field))
                    .BoundingBox(
                        GeoBounds.Coordinates(new CoordsGeoBounds
                        {
                            Bottom = boundingBox.BottomRight.Latitude,
                            Left = boundingBox.TopLeft.Longitude,
                            Right = boundingBox.BottomRight.Longitude,
                            Top = boundingBox.TopLeft.Latitude
                        })
                    )
                )
            );
        }
    }

    /// <summary>
    /// Try to add nested term criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="queries"></param>
    /// <param name="nestedPath"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public static void TryAddNestedTermCriteria<TQueryDescriptor, TValue>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, 
        string nestedPath, string field, TValue value) where TQueryDescriptor : class
    {
        if (value != null)
        {
            queries.Add(q => q
                .Nested(n => n
                    .Path(nestedPath)
                    .Query(q => q
                        .Term(t => t
                            .Field($"{nestedPath}.{field}")
                            .Value(value.ToFieldValue())
                        )
                    )
                )
            );
        }
    }

    /// <summary>
    /// Try to add nested terms criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="queries"></param>
    /// <param name="nestedPath"></param>
    /// <param name="field"></param>
    /// <param name="values"></param>
    public static void TryAddNestedTermsCriteria<TQueryDescriptor, TValue>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string nestedPath, string field, IEnumerable<TValue> values) where TQueryDescriptor : class
    {
        if (values?.Any() ?? false)
        {
            queries.Add(q => q
                .Nested(n => n
                    .Path(nestedPath)
                    .Query(q => q
                        .Terms(t => t
                            .Field($"{nestedPath}.{field}")
                            .Terms(new TermsQueryField(values.ToFieldValues()))
                        )
                    )
               )
            );
        }
    }

    /// <summary>
    ///  Add numeric range criteria if value is not null 
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    public static void TryAddNumericRangeCriteria<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, double? value, RangeTypes type) where TQueryDescriptor : class
    {
        if (value != null)
        {
            switch (type)
            {
                case RangeTypes.GreaterThan:
                    queries.Add(q => q
                        .Range(r => r
                            .NumberRange(nr => nr
                                .Field(field.ToField())
                                .Gt(value)
                            )
                        )
                    );
                    break;
                case RangeTypes.GreaterThanOrEquals:
                    queries.Add(q => q
                        .Range(r => r
                            .NumberRange(nr => nr
                                .Field(field.ToField())
                                .Gte(value)
                            )
                        )
                    );
                    break;
                case RangeTypes.LessThan:
                    queries.Add(q => q
                        .Range(r => r
                            .NumberRange(nr => nr
                                .Field(field.ToField())
                                .Lt(value)
                            )
                        )
                    );
                    break;
                case RangeTypes.LessThanOrEquals:
                    queries.Add(q => q
                        .Range(r => r
                            .NumberRange(nr => nr
                                .Field(field.ToField())
                                .Lte(value)
                            )
                        )
                    );
                    break;
            }
        }
    }

    /// <summary>
    /// Try add date range criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="dateTime"></param>
    /// <param name="type"></param>
    public static void TryAddDateRangeCriteria<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, DateTime? dateTime, RangeTypes type) where TQueryDescriptor : class
    {
        if (dateTime.HasValue)
        {
            switch (type)
            {
                case RangeTypes.GreaterThan:
                    queries.Add(q => q
                        .Range(r => r
                            .DateRange(dr => dr
                                .Field(field.ToField())
                                .Gt(DateMath.Anchored(dateTime.Value.ToUniversalTime()))
                            )
                        )
                    );
                    break;
                case RangeTypes.GreaterThanOrEquals:
                    queries.Add(q => q
                        .Range(r => r
                            .DateRange(dr => dr
                                .Field(field.ToField())
                                .Gte(DateMath.Anchored(dateTime.Value.ToUniversalTime()))
                            )
                        )
                    );
                    break;
                case RangeTypes.LessThan:
                    queries.Add(q => q
                        .Range(r => r
                            .DateRange(dr => dr
                                .Field(field.ToField())
                                .Lt(DateMath.Anchored(dateTime.Value.ToUniversalTime()))
                            )
                        )
                    );
                    break;
                case RangeTypes.LessThanOrEquals:
                    queries.Add(q => q
                       .Range(r => r
                            .DateRange(dr => dr
                                .Field(field.ToField())
                                .Lte(DateMath.Anchored(dateTime.Value.ToUniversalTime()))
                            )
                        )
                    );
                    break;
            }
        }
    }

    /// <summary>
    /// Add geo distance criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="point"></param>
    /// <param name="distanceType"></param>
    /// <param name="distance"></param>
    public static void TryAddGeoDistanceCriteria<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, Point point, GeoDistanceType distanceType, double distance) where TQueryDescriptor : class
    {
        if (distance != 0)
        {
            queries.Add(q => q
                .GeoDistance(gd => gd
                    .Distance($"{(int)distance}m")
                    .DistanceType(distanceType)
                    .Field(field)
                    .Location(point.ToGeoLocation())
                    .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                )
            );
        }
    }

    /// <summary>
    ///  Add geo shape criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="geometry"></param>
    /// <param name="relation"></param>
    public static void TryAddGeoShapeCriteria<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, Geometry geometry, GeoShapeRelation relation) where TQueryDescriptor : class
    {
        if (geometry?.IsValid ?? false)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
            var shape = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(geometry, options));
            var gsq = new GeoShapeQuery
            {
                Field = field.ToField(),
                Shape = new GeoShapeFieldQuery
                {
                    Relation = relation,
                    Shape = shape
                }
            };
           
            queries.Add(q => q
                .GeoShape(gsq)

            /*.GeoShape(gd => gd
                .Field(field)
                .Shape(s => s
                    .Shape(shape)
                    .Relation(relation)
                )
            )*/
            );
        }
    }

    /// <summary>
    /// Add nested term criteria with and
    /// </summary>
    /// <param name="queries"></param>
    /// <param name="nestedPath"></param>
    /// <param name="fieldValues"></param>
    public static void TryAddNestedTermAndCriteria<TQueryDescriptor>(this
        ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string nestedPath,
        IDictionary<string, object> fieldValues) where TQueryDescriptor : class
    {
        if (fieldValues?.Any() ?? false)
        {
            var nestedQueries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

            foreach (var fieldValue in fieldValues)
            {
                nestedQueries.TryAddTermCriteria($"{nestedPath}.{fieldValue.Key}", fieldValue.Value);
            }
            queries.Add(q => q
                .Nested(n => n
                    .Path(nestedPath)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(nestedQueries.ToArray())
                        )
                    )
                )
            );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <typeparam name="TTerms"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="terms"></param>
    public static void TryAddTermsCriteria<TQueryDescriptor, TTerms>(
                this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, IEnumerable<TTerms> terms) where TQueryDescriptor : class
    {
        if (terms?.Any() ?? false)
        {
            if (terms is IEnumerable<bool> boolValues)
            {
                queries.Add(q => q
                    .Terms(t => t
                        .Field(field)
                        .Terms(new TermsQueryField(boolValues.Select(b => FieldValue.Boolean(b)).ToArray()))
                    )
                );
            }
            else if (terms is IEnumerable<double> doubleValues)
            {
                queries.Add(q => q
                    .Terms(t => t
                        .Field(field)
                        .Terms(new TermsQueryField(doubleValues.Select(d => FieldValue.Double(d)).ToArray()))
                    )
                );
            }
            else if (terms is IEnumerable<long> longValues)
            {
                queries.Add(q => q
                    .Terms(t => t
                         .Field(field)
                         .Terms(new TermsQueryField(longValues.Select(b => FieldValue.Long(b)).ToArray()))
                     )
                );
            }
            else if (terms is IEnumerable<int> intValues)
            {
                queries.Add(q => q
                    .Terms(t => t
                        .Field(field)
                        .Terms(new TermsQueryField(intValues.Select(i => FieldValue.Long(i)).ToArray()))
                    )
                );
            }
            else if (terms is IEnumerable<short> shortValues)
            {
                queries.Add(q => q
                    .Terms(t => t
                         .Field(field)
                         .Terms(new TermsQueryField(shortValues.Select(s => FieldValue.Long(s)).ToArray()))
                     )
                );
            }
            else if (terms is IEnumerable<byte> byteValues)
            {
                queries.Add(q => q
                    .Terms(t => t
                        .Field(field)
                        .Terms(new TermsQueryField(byteValues.Select(b => FieldValue.Long(b)).ToArray()))
                    )
                );
            }
            else
            {
                queries.Add(q => q
                     .Terms(t => t
                        .Field(field)
                        .Terms(new TermsQueryField(terms.Select(s => FieldValue.String(s?.ToString())).ToArray()))
                    )
                );
            }   
        }
    }



    /// <summary>
    ///  Try to add query criteria
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public static void TryAddTermCriteria<TQueryDescriptor, TValue>(
        this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, TValue value) where TQueryDescriptor : class
    {
        if (!string.IsNullOrEmpty(value?.ToString()))
        {
            if (value is bool boolValue)
            {
                queries.Add(q => q
                    .Term(m => m
                        .Field(field)
                        .Value(FieldValue.Boolean(boolValue))
                    )
                );
            }
            else if (value is double doubleValue)
            {
                queries.Add(q => q
                    .Term(m => m
                        .Field(field)
                        .Value(FieldValue.Double(doubleValue))
                    )
                );
            }
            else if (value is long longValue)
            {
                queries.Add(q => q
                   .Term(m => m
                       .Field(field)
                       .Value(FieldValue.Long(longValue))
                   )
               );
            }
            else if (value is int intValue)
            {
                queries.Add(q => q
                   .Term(m => m
                       .Field(field)
                       .Value(FieldValue.Long(intValue))
                   )
               );
            }
            else if (value is short shortValue)
            {
                queries.Add(q => q
                  .Term(m => m
                      .Field(field)
                      .Value(FieldValue.Long(shortValue))
                  )
              );
            }
            else if (value is byte byteValue)
            {
                queries.Add(q => q
                     .Term(m => m
                         .Field(field)
                         .Value(FieldValue.Long(byteValue))
                     )
                );
            }
            else
            {
                queries.Add(q => q
                     .Term(m => m
                         .Field(field)
                         .Value(FieldValue.String(value?.ToString()))
                     )
                );
            }
        }
    }

    /// <summary>
    /// Try to add query criteria where property must match a specified value
    /// </summary>
    /// <typeparam name="TQueryDescriptor"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="matchValue"></param>
    public static void TryAddTermCriteria<TQueryDescriptor, TValue>(
       this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, TValue value, TValue matchValue) where TQueryDescriptor : class
    {
        if (!string.IsNullOrEmpty(value?.ToString()) && matchValue.Equals(value))
        {
            queries.TryAddTermCriteria(field, value);
        }
    }

    /// <summary>
    /// Add wildcard criteria
    /// </summary>
    /// <param name="queries"></param>
    /// <param name="field"></param>
    /// <param name="wildcard"></param>
    public static void TryAddWildcardCriteria<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries, string field, string wildcard) where TQueryDescriptor : class
    {
        if (!string.IsNullOrEmpty(wildcard))
        {
            queries.Add(q => q
                .Wildcard(w => w
                    .Field(field)
                    .CaseInsensitive(true)
                    .Value(wildcard)
                )
            );
        }
    }
}