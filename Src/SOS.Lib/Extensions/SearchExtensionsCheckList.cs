using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS.Lib;

/// <summary>
/// Observation specific search related extensions
/// </summary>
public static class SearchExtensionsChecklist
{
    extension<TQueryDescriptor>(ICollection<Action<QueryDescriptor<TQueryDescriptor>>> queries) where TQueryDescriptor : class
    {
        /// <summary>
        /// Add geometry filter to query
        /// </summary>
        /// <param name="geographicsFilter"></param>
        private void TryAddGeometryFilters(
            GeographicsFilter geographicsFilter)
        {
            if (geographicsFilter == null)
            {
                return;
            }

            var boundingBoxContainers = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
            if (!(!geographicsFilter.UsePointAccuracy && geographicsFilter.UseDisturbanceRadius))
            {
                boundingBoxContainers.TryAddBoundingBoxCriteria(
                        $"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}",
                        geographicsFilter.BoundingBox);
            }

            if (geographicsFilter.UseDisturbanceRadius)
            {
                // Add both point and pointWithDisturbanceBuffer, since pointWithDisturbanceBuffer can be null if no dist buffer exists
                boundingBoxContainers.TryAddBoundingBoxCriteria("location.point", geographicsFilter.BoundingBox);
                boundingBoxContainers.TryAddBoundingBoxCriteria("location.pointWithDisturbanceBuffer", geographicsFilter.BoundingBox);
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
                        geometryContainers.TryAddGeoDistanceCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", geom.Centroid, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0);

                        if (!geographicsFilter.UseDisturbanceRadius)
                        {
                            continue;
                        }
                        geometryContainers.TryAddGeoDistanceCriteria("location.pointWithDisturbanceBuffer", geom.Centroid, GeoDistanceType.Arc, geographicsFilter.MaxDistanceFromPoint ?? 0);
                        break;
                    case OgcGeometryType.Polygon:
                    case OgcGeometryType.MultiPolygon:
                        var vaildGeometry = geom.TryMakeValid();
                        geometryContainers.TryAddGeoShapeCriteria($"location.{(geographicsFilter.UsePointAccuracy ? "pointWithBuffer" : "point")}", vaildGeometry, geographicsFilter.UsePointAccuracy ? GeoShapeRelation.Intersects : GeoShapeRelation.Within);
                        if (!geographicsFilter.UseDisturbanceRadius)
                        {
                            continue;
                        }
                        geometryContainers.TryAddGeoShapeCriteria("location.pointWithDisturbanceBuffer", vaildGeometry, GeoShapeRelation.Intersects);
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
        /// <param name="geographicAreasFilter"></param>
        private ICollection<Action<QueryDescriptor<TQueryDescriptor>>> TryAddGeographicFilter(GeographicAreasFilter geographicAreasFilter)
        {
            if (geographicAreasFilter != null)
            {
                queries.TryAddTermsCriteria("location.countryRegion.featureId", geographicAreasFilter.CountryRegionIds);
                queries.TryAddTermsCriteria("location.county.featureId", geographicAreasFilter.CountyIds);
                queries.TryAddTermsCriteria("location.municipality.featureId", geographicAreasFilter.MunicipalityIds);
                queries.TryAddTermsCriteria("location.parish.featureId", geographicAreasFilter.ParishIds);
                queries.TryAddTermsCriteria("location.province.featureId", geographicAreasFilter.ProvinceIds);
                queries.TryAddGeometryFilters(geographicAreasFilter.GeometryFilter);
            }

            return queries;
        }
    }

    extension(ChecklistSearchFilter filter)
    {
        /// <summary>
        /// Create search filter
        /// </summary>
        /// <returns></returns>
        public ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToQuery<TQueryDescriptor>(
    ) where TQueryDescriptor : class
        {
            var queries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();

            if (filter != null)
            {
                queries.TryAddTermsCriteria("dataProviderId", filter.DataProviderIds);
                queries.TryAddScript((filter.Date?.MinEffortTime ?? TimeSpan.Zero) > TimeSpan.Zero ? $"return doc['event.endDate'].value.getMillis() - doc['event.startDate'].value.getMillis() >= {filter.Date.MinEffortTime.TotalMilliseconds}L;" : null);
                queries.TryAddTermsCriteria("taxonIds", filter.Taxa?.Ids);
                queries.TryAddTermsCriteria("projects.id", filter.ProjectIds);
                queries.TryAddEventDateCritera("event", filter.Date);
                queries.TryAddGeographicFilter(filter.Location?.AreaGeographic);
                queries.TryAddGeometryFilters(filter.Location?.Geometries);
            }

            return queries;
        }
    }
}