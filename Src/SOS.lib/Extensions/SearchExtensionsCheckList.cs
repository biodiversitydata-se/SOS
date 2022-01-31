using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search;

namespace SOS.Lib
{
    /// <summary>
    /// Observation specific search related extensions
    /// </summary>
    public static class SearchExtensionsCheckList
    {

        /// <summary>
        /// Add geometry filter to query
        /// </summary>
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="geographicsFilter"></param>
        private static void TryAddGeometryFilters<TQueryContainer>(
            this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query,
            GeographicsFilter geographicsFilter) where TQueryContainer : class
        {
            if (geographicsFilter == null)
            {
                return;
            }

            var boundingBoxContainers = new List<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>>();

            if (!(!geographicsFilter.UsePointAccuracy && geographicsFilter.UseDisturbanceRadius))
            {
                boundingBoxContainers.TryAddBoundingBoxCriteria(
                    $"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}",
                    geographicsFilter.BoundingBox);
            }

            if (geographicsFilter.UseDisturbanceRadius)
            {
                // Add both point and pointWithDisturbanceBuffer, since pointWithDisturbanceBuffer can be null if no dist buffer exists
                boundingBoxContainers.TryAddBoundingBoxCriteria(
                    "location.point",
                    geographicsFilter.BoundingBox);

                boundingBoxContainers.TryAddBoundingBoxCriteria(
                    "location.pointWithDisturbanceBuffer",
                    geographicsFilter.BoundingBox);
            }

            if (boundingBoxContainers.Any())
            {
                query.Add(q => q
                    .Bool(b => b
                        .Should(boundingBoxContainers)
                    )
                );
            }

            if (!geographicsFilter?.IsValid ?? true)
            {
                return;
            }
            var geometryContainers = new List<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>>();

            foreach (var geom in geographicsFilter.Geometries)
            {
                switch (geom.Type.ToLower())
                {
                    case "point":
                        geometryContainers.AddGeoDistanceCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", geom, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0);

                        if (!geographicsFilter.UseDisturbanceRadius)
                        {
                            continue;
                        }
                        geometryContainers.AddGeoDistanceCriteria("location.pointWithDisturbanceBuffer", geom, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0);
                        break;
                    case "polygon":
                    case "multipolygon":
                        var vaildGeometry = geom.TryMakeValid();
                        geometryContainers.AddGeoShapeCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", vaildGeometry, geographicsFilter.UsePointAccuracy ? GeoShapeRelation.Intersects : GeoShapeRelation.Within);
                        if (!geographicsFilter.UseDisturbanceRadius)
                        {
                            continue;
                        }
                        geometryContainers.AddGeoShapeCriteria("location.pointWithDisturbanceBuffer", vaildGeometry, GeoShapeRelation.Intersects);
                        break;
                }
            }

            query.Add(q => q
                .Bool(b => b
                    .Should(geometryContainers)
                )
            );
        }

        /// <summary>
        /// Try to add geographical filter
        /// </summary>
        /// <typeparam name="TQueryContainer"></typeparam>
        /// <param name="query"></param>
        /// <param name="geographicAreasFilter"></param>
        private static void TryAddGeographicFilter<TQueryContainer>(this ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> query,
            GeographicAreasFilter geographicAreasFilter) where TQueryContainer : class
        {
            if (geographicAreasFilter == null)
            {
                return;
            }

            query.TryAddTermsCriteria("location.county.featureId", geographicAreasFilter.CountyIds);
            query.TryAddTermsCriteria("location.municipality.featureId", geographicAreasFilter.MunicipalityIds);
            query.TryAddTermsCriteria("location.parish.featureId", geographicAreasFilter.ParishIds);
            query.TryAddTermsCriteria("location.province.featureId", geographicAreasFilter.ProvinceIds);

            query.TryAddGeometryFilters(geographicAreasFilter.GeometryFilter);
        }


        /// <summary>
        ///     Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> ToQuery<TQueryContainer>(
            this CheckListSearchFilter filter) where TQueryContainer : class
        {
            var query = new List<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>>();

            if (filter == null)
            {
                return query;
            }

            query.TryAddTermsCriteria("dataProviderId", filter.DataProviderIds);
            query.TryAddEventDateCritera(filter.Date, "event");

            if ((filter.Date?.MinEffortTime ?? TimeSpan.Zero) > TimeSpan.Zero)
            {
                query.AddScript($"return doc['event.endDate'].value.getMillis() - doc['event.startDate'].value.getMillis() >= {filter.Date.MinEffortTime.TotalMilliseconds}L;");
            }

            query.TryAddGeographicFilter(filter.Location?.AreaGeographic);
            query.TryAddGeometryFilters(filter.Location?.Geometries);
            query.TryAddTermsCriteria("taxonIds", filter.Taxa?.Ids);
            query.TryAddNestedTermsCriteria("projects", "id", filter.ProjectIds);

            return query;
        }
    }
}