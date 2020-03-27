using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
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

            /*  if (filter.GeometryFilter?.IsValid ?? false)
              {
                  var geoJsonGeometry = filter.GeometryFilter.Geometry.ToGeoJsonGeometry(); ;

                  switch (geoJsonGeometry.Type)
                  {
                      case GeoJsonObjectType.Point:
                          filters.Add(Builders<ProcessedObservation>.Filter.Near(m => m.Location.Point, (GeoJsonPoint<GeoJson2DGeographicCoordinates>)geoJsonGeometry, filter.GeometryFilter.MaxDistanceFromPoint, 0.0));
                          break;
                      case GeoJsonObjectType.Polygon:
                      case GeoJsonObjectType.MultiPolygon:
                          filters.Add(filter.GeometryFilter.UsePointAccuracy ?
                              Builders<ProcessedObservation>.Filter.GeoIntersects(m => m.Location.PointWithBuffer, geoJsonGeometry)
                              :
                              Builders<ProcessedObservation>.Filter.GeoWithin(m => m.Location.Point, geoJsonGeometry));

                          break;
                  }
              }*/

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
            // filters.Add(Builders<ProcessedObservation>.Filter.ElemMatch(o => o.Projects, o => o.ProjectParameters != null));
            //query.Add(Todo);
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
