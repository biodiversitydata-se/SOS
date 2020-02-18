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
        private static List<FilterDefinition<ProcessedSighting>> CreateFilterDefinitions(AdvancedFilter filter)
        {
            var filters = new List<FilterDefinition<ProcessedSighting>>();

            if (filter.Counties?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Location.CountyId.Id, filter.Counties));
            }

            if (filter.Delimitation?.IsValid ?? false)
            {
                var geoJsonGeometry = filter.Delimitation.Geometry.ToGeoJsonGeometry(); ;

                switch (geoJsonGeometry.Type)
                {
                    case GeoJsonObjectType.Point:
                        filters.Add(Builders<ProcessedSighting>.Filter.Near(m => m.Location.Point, (GeoJsonPoint<GeoJson2DGeographicCoordinates>)geoJsonGeometry, filter.Delimitation.MaxDistanceFromPoint, 0.0 ));
                        break;
                    case GeoJsonObjectType.Polygon:
                    case GeoJsonObjectType.MultiPolygon:
                        filters.Add(filter.Delimitation.UsePointAccuracy ? 
                            Builders<ProcessedSighting>.Filter.GeoIntersects(m => m.Location.PointWithBuffer, geoJsonGeometry)
                            :
                            Builders<ProcessedSighting>.Filter.GeoWithin(m => m.Location.Point, geoJsonGeometry));
                        
                        break;
                }
            }

            if (filter.EndDate.HasValue)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.Lte(m => m.Event.EndDate, filter.EndDate.Value.ToUniversalTime()));
            }

            if (filter.OnlyValidated.HasValue && filter.OnlyValidated.Value.Equals(true))
            {
                filters.Add(
                    Builders<ProcessedSighting>.Filter.Eq(m => m.Identification.Validated, true));
            }

            if (filter.Municipalities?.Any() ?? false)
            {
                filters.Add(
                    Builders<ProcessedSighting>.Filter.In(m => m.Location.MunicipalityId.Id, filter.Municipalities));
            }

            if (filter.PositiveSightings.HasValue)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.Eq(m => m.Occurrence.IsPositiveObservation, filter.PositiveSightings.Value));
            }

            if (filter.Provinces?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Location.ProvinceId.Id, filter.Provinces));
            }

            if (filter.RedListCategories?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Taxon.RedlistCategory, filter.RedListCategories));
            }

            if (filter.Sex?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Occurrence.Sex.Id, filter.Sex));
            }

            if (filter.StartDate.HasValue)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.Gte(m => m.Event.StartDate, filter.StartDate.Value.ToUniversalTime()));
            }

            if (filter.TaxonIds?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Taxon.Id, filter.TaxonIds));
            }

            return filters;
        }

        /// <summary>
        /// Create project parameter filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FilterDefinition<ProcessedSighting> ToProjectParameteFilterDefinition(this AdvancedFilter filter)
        {
            var filters = CreateFilterDefinitions(filter);
            filters.Add(Builders<ProcessedSighting>.Filter.ElemMatch(
                o => o.Projects, o => o.ProjectParameters != null));

            return Builders<ProcessedSighting>.Filter.And(filters);
        }

        /// <summary>
        /// Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FilterDefinition<ProcessedSighting> ToFilterDefinition(this AdvancedFilter filter)
        {
            if (!filter.IsFilterActive)
            {
                return FilterDefinition<ProcessedSighting>.Empty;
            }

            var filters = CreateFilterDefinitions(filter);
            return Builders<ProcessedSighting>.Filter.And(filters);
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
