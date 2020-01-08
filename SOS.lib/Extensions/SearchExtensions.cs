using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    public static class SearchExtensions
    {
        /// <summary>
        /// Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FilterDefinition<DarwinCore<DynamicProperties>> ToFilterDefinition(this AdvancedFilter filter)
        {
            if (!filter.IsFilterActive)
            {
                return FilterDefinition<DarwinCore<DynamicProperties>>.Empty;
            }

            var filters = new List<FilterDefinition<DarwinCore<DynamicProperties>>>();

            if (filter.TaxonIds?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Taxon.Id, filter.TaxonIds));
            }

            if (filter.StartDate.HasValue)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.Gte(m => m.Event.EventDate, $"{filter.StartDate.Value.ToUniversalTime().ToString("s")}Z"));
            }

            if (filter.EndDate.HasValue)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.Lte(m => m.Event.EventDate, $"{filter.EndDate.Value.ToUniversalTime().ToString("s")}Z"));
            }

            if (filter.Counties?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Location.County, filter.Counties));
            }

            if (filter.Municipalities?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Location.Municipality, filter.Municipalities));
            }

            if (filter.Provinces?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Location.StateProvince, filter.Provinces));
            }

            if (filter.Sex?.Any() ?? false)
            {
                filters.Add(Builders<DarwinCore<DynamicProperties>>.Filter.In(m => m.Occurrence.Sex, filter.Sex));
            }

            return Builders<DarwinCore<DynamicProperties>>.Filter.And(filters);
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
