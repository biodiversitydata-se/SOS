﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    public static class SearchExtensions
    {
        private static List<Func<QueryContainerDescriptor<ProcessedObservation>, QueryContainer>> CreateQuery(FilterBase filter)
        {
            var queryContainers = new List<Func<QueryContainerDescriptor<ProcessedObservation>, QueryContainer>>();

            if (filter.CountyIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.County.Id)
                        .Terms(filter.CountyIds)
                    )
                );
            }

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                switch (filter.GeometryFilter.Geometry.Type.ToLower())
                {
                    case "point":
                        queryContainers.Add(q => q
                            .GeoDistance(gd => gd
                                .Field(f => f.Location.PointLocation)
                                .DistanceType(GeoDistanceType.Arc)
                                .Location(filter.GeometryFilter.Geometry.ToGeoLocation())
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
                                    .Field(f => f.Location.PointWithBuffer)
                                    .Shape(s => filter.GeometryFilter.Geometry.ToGeoShape())
                                    .Relation(GeoShapeRelation.Intersects)
                                )
                            );
                        }
                        else
                        {
                            queryContainers.Add(q => q
                                .GeoShape(gd => gd
                                    .Field(f => f.Location.Point)
                                    .Shape(s => filter.GeometryFilter.Geometry.ToGeoShape())
                                    .Relation(GeoShapeRelation.Within)
                                )
                            );
                        }
                        break;
                }
            }

            if (filter.EndDate.HasValue)
            {
                queryContainers.Add((QueryContainerDescriptor<ProcessedObservation> q) => q
                    .Range(r => r
                        .Field((y => y.Event.EndDate != null))
                        .Field(f => f.Event.EndDate)
                        .LessThanOrEquals(filter.EndDate.Value.ToUniversalTime().Ticks)
                    )
                );
            }

            if (filter.GenderIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Occurrence.Gender.Id)
                        .Terms(filter.GenderIds)
                    )
                );
            }

            if (filter.MunicipalityIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.Municipality.Id)
                        .Terms(filter.MunicipalityIds)
                    )
                );
            }

            if (filter.OnlyValidated.HasValue && filter.OnlyValidated.Value.Equals(true))
            {
                queryContainers.Add(q => q
                    .Term(m => m.Field(f => f.Identification.Validated).Value(true)));
            }

            if (filter.PositiveSightings.HasValue)
            {
                queryContainers.Add(q => q
                    .Term(m => m.Field(f => f.Occurrence.IsPositiveObservation).Value(filter.PositiveSightings.Value)));
            }

            if (filter.ProvinceIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Location.Province.Id)
                        .Terms(filter.ProvinceIds)
                    )
                );
            }

            if (filter.RedListCategories?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Taxon.RedlistCategory)
                        .Terms(filter.RedListCategories)
                    )
                );
            }

            if (filter.StartDate.HasValue)
            {
                queryContainers.Add(q => q
                    .Range(r => r
                        .Field((y => y.Event.StartDate != null))
                        .Field(f => f.Event.StartDate)
                        .GreaterThanOrEquals(filter.StartDate.Value.ToUniversalTime().Ticks)
                    )
                );
            }

            if (filter.TaxonIds?.Any() ?? false)
            {
                queryContainers.Add(q => q
                    .Terms(t => t
                        .Field(f => f.Taxon.Id)
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
        public static IEnumerable<Func<QueryContainerDescriptor<ProcessedObservation>, QueryContainer>> ToProjectParameteQuery(this FilterBase filter)
        {
            var query = CreateQuery(filter);

            query.Add(q => q
                .Nested(n => n
                        .Path(p => p.Projects)
                        .Query(q => q
                            .Exists(e => 
                                e.Field(f => f.Projects.Where(prj => prj.ProjectParameters.Any()))
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
        public static IEnumerable<Func<QueryContainerDescriptor<ProcessedObservation>, QueryContainer>> ToQuery(this FilterBase filter)
        {
            if (!filter.IsFilterActive)
            {
                return new List<Func<QueryContainerDescriptor<ProcessedObservation>, QueryContainer>>();
            }

            return CreateQuery(filter);
        }


        /// <summary>
        /// Build a projection string
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string ToProjection(this IEnumerable<string> fields)
        {
            var projection = $"{{ _id: 0, { string.Join(",", fields?.Where(f => !string.IsNullOrEmpty(f)).Select((f, i) => $"'{f}': {i + 1}") ?? new string[0]) } }}";
            return projection;
        }
    }
}
