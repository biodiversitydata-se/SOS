using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    public static class SearchExtensions
    {
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

        private static List<FilterDefinition<ProcessedSighting>> CreateFilterDefinitions(AdvancedFilter filter)
        {
            var filters = new List<FilterDefinition<ProcessedSighting>>();

            if (filter.TaxonIds?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Taxon.Id, filter.TaxonIds));
            }

            if (filter.StartDate.HasValue)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.Gte(m => m.Event.StartDate, filter.StartDate.Value));
            }

            if (filter.EndDate.HasValue)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.Lte(m => m.Event.EndDate, filter.EndDate));
            }

            if (filter.Counties?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Location.County.Name, filter.Counties));
            }

            if (filter.Municipalities?.Any() ?? false)
            {
                filters.Add(
                    Builders<ProcessedSighting>.Filter.In(m => m.Location.Municipality.Name, filter.Municipalities));
            }

            if (filter.Provinces?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Location.Province.Name, filter.Provinces));
            }

            if (filter.Sex?.Any() ?? false)
            {
                filters.Add(Builders<ProcessedSighting>.Filter.In(m => m.Occurrence.Sex, filter.Sex));
            }

            return filters;
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
