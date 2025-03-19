using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib
{
    /// <summary>
    /// Observation specific search related extensions
    /// </summary>
    public static class SearchExtensionsChecklist
    {

        /// <summary>
        /// Add geometry filter to query
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="queries"></param>
        /// <param name="geographicsFilter"></param>
        private static void TryAddGeometryFilters<TQueryDescriptor>(
            this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            GeographicsFilter geographicsFilter) where TQueryDescriptor : class
        {
            if (geographicsFilter == null)
            {
                return;
            }

            var boundingBoxContainers = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
            if (!(!geographicsFilter.UsePointAccuracy && geographicsFilter.UseDisturbanceRadius))
            {
                boundingBoxContainers.Add(q => q
                    .TryAddBoundingBoxCriteria(
                        $"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}",
                        geographicsFilter.BoundingBox)
                );
            }

            if (geographicsFilter.UseDisturbanceRadius)
            {
                // Add both point and pointWithDisturbanceBuffer, since pointWithDisturbanceBuffer can be null if no dist buffer exists
                boundingBoxContainers.Add(q => q
                   .TryAddBoundingBoxCriteria("location.point", geographicsFilter.BoundingBox)
                );
                boundingBoxContainers.Add(q => q
                   .TryAddBoundingBoxCriteria("location.pointWithDisturbanceBuffer", geographicsFilter.BoundingBox)
                );
            }

            if (boundingBoxContainers.Any())
            {
                queries.Add(q => q
                    .Bool(b => b
                        .Should(boundingBoxContainers.ToArray())
                    )
                );
            }

            if (!geographicsFilter?.IsValid ?? true)
            {
                return;
            }
            var geometryContainers = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

            foreach (var geom in geographicsFilter.Geometries)
            {
                switch (geom.OgcGeometryType)
                {
                    case OgcGeometryType.Point:
                        geometryContainers.Add(q => q
                            .AddGeoDistanceCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", geom.Centroid, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0)
                        );

                        if (!geographicsFilter.UseDisturbanceRadius)
                        {
                            continue;
                        }
                        geometryContainers.Add(q => q
                            .AddGeoDistanceCriteria("location.pointWithDisturbanceBuffer", geom.Centroid, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0)
                        );
                        break;
                    case OgcGeometryType.Polygon:
                    case OgcGeometryType.MultiPolygon:
                        var vaildGeometry = geom.TryMakeValid();
                        geometryContainers.Add(q => q
                            .AddGeoShapeCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", vaildGeometry, geographicsFilter.UsePointAccuracy ? GeoShapeRelation.Intersects : GeoShapeRelation.Within)
                        );
                        if (!geographicsFilter.UseDisturbanceRadius)
                        {
                            continue;
                        }
                        geometryContainers.Add(q => q
                            .AddGeoShapeCriteria("location.pointWithDisturbanceBuffer", vaildGeometry, GeoShapeRelation.Intersects)
                        );
                        break;
                }
            }

            queries.Add(q => q
                .Bool(b => b
                    .Should(geometryContainers.ToArray())
                )
            );
        }

        /// <summary>
        /// Try to add geographical filter
        /// </summary>
        /// <typeparam name="TQueryDescriptor"></typeparam>
        /// <param name="queries"></param>
        /// <param name="geographicAreasFilter"></param>
        private static void TryAddGeographicFilter<TQueryDescriptor>(this ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries,
            GeographicAreasFilter geographicAreasFilter) where TQueryDescriptor : class
        {
            if (geographicAreasFilter == null)
            {
                return;
            }
            queries.Add(q => q
                .TryAddTermsCriteria("location.countryRegion.featureId", geographicAreasFilter.CountryRegionIds)
                .TryAddTermsCriteria("location.county.featureId", geographicAreasFilter.CountyIds)
                .TryAddTermsCriteria("location.municipality.featureId", geographicAreasFilter.MunicipalityIds)
                .TryAddTermsCriteria("location.parish.featureId", geographicAreasFilter.ParishIds)
                .TryAddTermsCriteria("location.province.featureId", geographicAreasFilter.ProvinceIds)
            );

            queries.TryAddGeometryFilters(geographicAreasFilter.GeometryFilter);
        }


        /// <summary>
        ///     Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToQuery<TQueryDescriptor>(
            this ChecklistSearchFilter filter) where TQueryDescriptor : class
        {
            var query = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

            if (filter == null)
            {
                return query;
            }

            query.Add(q => q
                .TryAddTermsCriteria("dataProviderId", filter.DataProviderIds)
                .TryAddScript((filter.Date?.MinEffortTime ?? TimeSpan.Zero) > TimeSpan.Zero ? $"return doc['event.endDate'].value.getMillis() - doc['event.startDate'].value.getMillis() >= {filter.Date.MinEffortTime.TotalMilliseconds}L;" : null)
                .TryAddTermsCriteria("taxonIds", filter.Taxa?.Ids)
                .TryAddTermsCriteria("projects.id", filter.ProjectIds)
            );

            query.TryAddGeographicFilter(filter.Location?.AreaGeographic);
            query.TryAddGeometryFilters(filter.Location?.Geometries);
    
            query.TryAddEventDateCritera(filter.Date, "event").;

            return query;
        }
    }
}