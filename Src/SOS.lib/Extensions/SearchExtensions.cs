using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    public static class SearchExtensions
    {
        /// <summary>
        /// Create a filter definition object
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static List<FilterDefinition<ProcessedObservation>> CreateFilterDefinitions(FilterBase filter)
        {
            var filters = new List<FilterDefinition<ProcessedObservation>>();

            if (filter.CountyIds?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedObservation>.Filter.In(m => m.Location.CountyId.Id, filter.CountyIds));
            }

            if (filter.GeometryFilter?.IsValid ?? false)
            {
                var geoJsonGeometry = filter.GeometryFilter.Geometry.ToGeoJsonGeometry(); ;

                switch (geoJsonGeometry.Type)
                {
                    case GeoJsonObjectType.Point:
                        filters.Add(Builders<ProcessedObservation>.Filter.Near(m => m.Location.Point, (GeoJsonPoint<GeoJson2DGeographicCoordinates>)geoJsonGeometry, filter.GeometryFilter.MaxDistanceFromPoint, 0.0 ));
                        break;
                    case GeoJsonObjectType.Polygon:
                    case GeoJsonObjectType.MultiPolygon:
                        filters.Add(filter.GeometryFilter.UsePointAccuracy ? 
                            Builders<ProcessedObservation>.Filter.GeoIntersects(m => m.Location.PointWithBuffer, geoJsonGeometry)
                            :
                            Builders<ProcessedObservation>.Filter.GeoWithin(m => m.Location.Point, geoJsonGeometry));
                        
                        break;
                }
            }

            if (filter.EndDate.HasValue)
            {
                filters.Add(Builders<ProcessedObservation>.Filter.Lte(m => m.Event.EndDate, filter.EndDate.Value.ToUniversalTime()));
            }

            if (filter.OnlyValidated.HasValue && filter.OnlyValidated.Value.Equals(true))
            {
                filters.Add(
                    Builders<ProcessedObservation>.Filter.Eq(m => m.Identification.Validated, true));
            }

            if (filter.MunicipalityIds?.Any() ?? false)
            {
                filters.Add(
                    Builders<ProcessedObservation>.Filter.In(m => m.Location.MunicipalityId.Id, filter.MunicipalityIds));
            }

            if (filter.PositiveSightings.HasValue)
            {
                filters.Add(Builders<ProcessedObservation>.Filter.Eq(m => m.Occurrence.IsPositiveObservation, filter.PositiveSightings.Value));
            }

            if (filter.ProvinceIds?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedObservation>.Filter.In(m => m.Location.ProvinceId.Id, filter.ProvinceIds));
            }

            if (filter.RedListCategories?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedObservation>.Filter.In(m => m.Taxon.RedlistCategory, filter.RedListCategories));
            }

            if (filter.GenderIds?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedObservation>.Filter.In(m => m.Occurrence.GenderId.Id, filter.GenderIds));
            }

            if (filter.StartDate.HasValue)
            {
                filters.Add(Builders<ProcessedObservation>.Filter.Gte(m => m.Event.StartDate, filter.StartDate.Value.ToUniversalTime()));
            }

            if (filter.TaxonIds?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedObservation>.Filter.In(m => m.Taxon.Id, filter.TaxonIds));
            }

            return filters;
        }

        /// <summary>
        /// Create project parameter filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FilterDefinition<ProcessedObservation> ToProjectParameteFilterDefinition(this FilterBase filter)
        {
            var filters = CreateFilterDefinitions(filter);
            filters.Add(Builders<ProcessedObservation>.Filter.ElemMatch(
                o => o.Projects, o => o.ProjectParameters != null));

            return Builders<ProcessedObservation>.Filter.And(filters);
        }

        /// <summary>
        /// Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FilterDefinition<ProcessedObservation> ToFilterDefinition(this FilterBase filter)
        {
            if (!filter.IsFilterActive)
            {
                return FilterDefinition<ProcessedObservation>.Empty;
            }

            var filters = CreateFilterDefinitions(filter);
            return Builders<ProcessedObservation>.Filter.And(filters);
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
