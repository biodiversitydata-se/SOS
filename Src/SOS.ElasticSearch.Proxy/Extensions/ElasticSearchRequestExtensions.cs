using SOS.Lib.Models.Processed.Observation;

namespace SOS.ElasticSearch.Proxy.Extensions
{
    public static class ElasticSearchRequestExtensions
    {
        /// <summary>
        /// Remove some fields from search response
        /// </summary>
        /// <param name="bodyDictionary"></param>
        public static void UpdateExclude(this IDictionary<string, object>? bodyDictionary, IEnumerable<string> excludeFields)
        {
            if (bodyDictionary == null || !(excludeFields?.Any() ?? true))
            {
                return;
            }

            var uniqueExcludeFields = new HashSet<object>(excludeFields!);
            if (bodyDictionary.TryGetValue("_source", out dynamic? source))
            {
                if (((IDictionary<string, object>)source).TryGetValue("exclude", out var exclude))
                {
                    var queryExcludeFields = (IEnumerable<object>)exclude;
                    foreach(var queryExcludeField in queryExcludeFields)
                    {
                        uniqueExcludeFields.Add(queryExcludeField);
                    }
                }
            }

            bodyDictionary["_source"] = new { exclude = uniqueExcludeFields };
        }

        /// <summary>
        /// Update query with mandatory search parameters
        /// </summary>
        /// <param name="bodyDictionary"></param>
        public static void UpdateQuery(this IDictionary<string, object>? bodyDictionary)
        {
            if (bodyDictionary == null)
            {
                return;
            }

            IDictionary<string, object> queryDictionary = new Dictionary<string, object>();
            IDictionary<string, object> boolDictionary = new Dictionary<string, object>();
            ICollection<IDictionary<string, object>> filterList = new List<IDictionary<string, object>>();
            if (bodyDictionary.TryGetValue("query", out var query))
            {
                queryDictionary = (IDictionary<string, object>)query;
                bodyDictionary.Remove("query"); // Remove query since we gone rebuild it later

                if (queryDictionary.TryGetValue("bool", out var @bool))
                {
                    boolDictionary = (IDictionary<string, object>)@bool!;
                    queryDictionary.Remove("bool"); // Remove bool since we gone rebuild it later

                    if (boolDictionary.TryGetValue("filter", out var filter))
                    {
                        if (filter.GetType().Name.Contains("List", StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (var dictionary in (IEnumerable<object>)filter)
                            {
                                filterList.Add((IDictionary<string, object>)dictionary);
                            }
                        }
                        else
                        {
                            filterList.Add((IDictionary<string, object>)filter);
                        }
                        boolDictionary.Remove("filter"); // Remove filter since we gone rebuild it later
                    }
                }
                else // Leaf Query
                {
                    foreach (var searchParam in queryDictionary)
                    {
                        filterList.Add(new Dictionary<string, object>() { { searchParam.Key, (IDictionary<string, object>)searchParam.Value } });
                        queryDictionary.Remove(searchParam.Key);
                    }
                }
            }

            ICollection<IDictionary<string, object>> sightingTypeQuery = new List<IDictionary<string, object>>();
            // Only Artportalen observations with specified default sightingTypeSearchGroupId
            sightingTypeQuery.Add(new Dictionary<string, object>() { { "terms", 
                new Dictionary<string, object>() { { "artportalenInternal.sightingTypeSearchGroupId", 
                    new[] { 1, 4, 16, 32, 128 } } } } }
            );// Non AP observations don't have sightingTypeSearchGroupId, get them as well
            sightingTypeQuery.Add(
                new Dictionary<string, object>() { { "bool", 
                    new Dictionary<string, object>() { { "must_not", 
                            new Dictionary<string, object>() { { "exists", 
                                new { field = "artportalenInternal.sightingTypeSearchGroupId" } } } } } } }
            );
            // Add OR query
            filterList.Add(
                new Dictionary<string, object>() { { "bool",
                    new Dictionary<string, object>() { { "should", sightingTypeQuery } } } }
            );

            boolDictionary.Add("filter", filterList);
            queryDictionary.Add("bool", boolDictionary);
            bodyDictionary.Add("query", queryDictionary);
        }

        /// <summary>
        /// Chenge sort to event en date if _id is passed
        /// </summary>
        /// <param name="bodyDictionary"></param>
        public static void UpdateSort(this IDictionary<string, object>? bodyDictionary)
        {
            if (bodyDictionary == null)
            {
                return;
            }

            if (bodyDictionary.TryGetValue("sort", out dynamic? sort))
            {
                var sortDictionary = (IDictionary<string, object>)sort;
                if (sortDictionary.ContainsKey("_id"))
                {
                    sortDictionary.Add("event.endDate", new { order = "desc" });
                    sortDictionary.Remove("_id");
                    bodyDictionary["sort"] = sortDictionary;
                }
            }
        }
    }
}
