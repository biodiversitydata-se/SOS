using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    public static class SearchExtensions
    {        
        private static List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> CreateQuery(FilterBase filter)
        {
            var queryContainers = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();

            if (filter.CountyIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.county.id")
                        .Terms(filter.CountyIds)
                    )
                );
            }

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                foreach (var geom in filter.GeometryFilter.Geometries)
                {
                    switch (geom.Type.ToLower())
                    {
                        case "point":
                            queryContainers.Add(q => q
                                .GeoDistance(gd => gd
                                    .Field("location.pointLocation")
                                    .DistanceType(GeoDistanceType.Arc)
                                    .Location(geom.ToGeoLocation())
                                    .Distance(filter.GeometryFilter.MaxDistanceFromPoint ?? 0, DistanceUnit.Meters)
                                    .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                )
                            );

                            break;
                        case "polygon":
                        case "multipolygon":
                            if (filter.GeometryFilter.UsePointAccuracy)
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.pointWithBuffer")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Intersects)
                                    )
                                );
                            }
                            else
                            {
                                queryContainers.Add(q => q
                                    .GeoShape(gd => gd
                                        .Field("location.point")
                                        .Shape(s => geom)
                                        .Relation(GeoShapeRelation.Within)
                                    )
                                );
                            }
                            break;
                    }
                }
            }

            if (filter.EndDate.HasValue)
            {
                queryContainers.Add(q => q
                    .DateRange(r => r
                        .Field("event.endDate")
                        .LessThanOrEquals(filter.EndDate.Value.ToUniversalTime())
                    )
                );
            }

            if (filter.GenderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("occurrence.gender.id")
                        .Terms(filter.GenderIds)
                    )
                );
            }

            if (filter.MunicipalityIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.municipality.id")
                        .Terms(filter.MunicipalityIds)
                    )
                );
            }

            if (filter.OnlyValidated.HasValue && filter.OnlyValidated.Value.Equals(true))
            {
                queryContainers.Add(q => q
                    .Term(m => m.Field("identification.validated").Value(true)));
            }

            if (filter.PositiveSightings.HasValue)
            {
                queryContainers.Add(q => q
                    .Term(m => m.Field("occurrence.isPositiveObservation").Value(filter.PositiveSightings.Value)));
            }

            if (filter.Providers?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("provider")
                        .Terms(filter.Providers)
                    )
                );
            }

            if (filter.ProvinceIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("location.province.id")
                        .Terms(filter.ProvinceIds)
                    )
                );
            }

            if (filter.RedListCategories?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("taxon.redlistCategory")
                        .Terms(filter.RedListCategories)
                    )
                );
            }

            if (filter.StartDate.HasValue)
            {
                queryContainers.Add(q => q
                    .DateRange(r => r
                        .Field("event.startDate")
                        .GreaterThanOrEquals(filter.StartDate.Value.ToUniversalTime())
                    )
                );
            }

            if (filter.TaxonIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field("taxon.id")
                        .Terms(filter.TaxonIds)
                    )
                );
            }

            return queryContainers;
        }


        /// <summary>
        /// Create project parameter filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToProjectParameterQuery(this FilterBase filter)
        {
            var query = CreateQuery(filter);
            query.Add(q => q
                .Nested(n => n
                    .Path("projects")
                    .Query(q => q
                        .Nested(n => n
                            .Path("projects.projectParameters")
                            .Query(q => q
                                .Exists(e => e
                                    .Field("projects.projectParameters")
                                )
                            )
                        )
                    )
                )
            );

            return query;
        }

        /// <summary>
        /// Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IEnumerable<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToQuery(this FilterBase filter)
        {
            if (!filter.IsFilterActive)
            {
                return new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
            }

            return CreateQuery(filter);
        }


        /// <summary>
        /// Build a projection string
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static Func<SourceFilterDescriptor<dynamic>, ISourceFilter> ToProjection(this IEnumerable<string> fields)
        {
            var projection = new SourceFilterDescriptor<dynamic>()
                   .Excludes(e => e
                       .Field("defects")
                       .Field("location.point")
                       .Field("location.pointLocation")
                       .Field("location.pointWithBuffer")
               );

            if (fields?.Any() ?? false)
            {
                projection.Includes(i => i
                    .Fields(fields.Select(f =>new Field(f) ))
                );
            }

            return p => projection;
        }
    }
}
