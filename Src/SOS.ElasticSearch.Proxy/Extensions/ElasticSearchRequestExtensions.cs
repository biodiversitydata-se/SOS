

namespace SOS.ElasticSearch.Proxy.Extensions;

public static class ElasticSearchRequestExtensions
{
    private static bool CheckIfCriteriaExists(IEnumerable<IDictionary<string, object>> criterias, string property)
    {
        var criteriaExists = false;
        foreach (var criteria in criterias)
        {
            foreach (var dictionary in criteria)
            {
                var dic = dictionary.Value as IDictionary<string, object>;
                if (dic != null && dic.Keys.Count(k => k.Equals(property, StringComparison.CurrentCultureIgnoreCase)) != 0)
                {
                    criteriaExists = true;
                    break;
                }
            }
        }

        return criteriaExists;
    }

    private static ICollection<IDictionary<string, object>> TryOptimizeQuery(ICollection<IDictionary<string, object>> shouldQuery, ICollection<IDictionary<string, object>> filterQuery)
    {
        if ((shouldQuery?.Count() ?? 0) == 0)
        {
            return shouldQuery!;
        }
        var cleanedQuery = new List<IDictionary<string, object>>();
        var termCriterias = new Dictionary<string, IList<object>>();
        foreach (var part in shouldQuery!)
        {
            bool addPart = true;
            foreach (var criteria in part)
            {
                if (criteria.Key.Equals("term"))
                {
                    var term = (IDictionary<string, object>)criteria.Value;
                    var key = term.Keys.FirstOrDefault()?.ToString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        if (!termCriterias.TryGetValue(key, out var termValue))
                        {
                            termValue = new List<object>();
                            termCriterias.Add(key, termValue);
                        }
                        var value = term.Values?.FirstOrDefault();
                        if (value != null)
                        {
                            termValue!.Add(value);
                            addPart = false;
                        }
                    }
                }
            }
            if (addPart)
            {
                cleanedQuery.Add(part);
            }
        }
        foreach (var termCriteria in termCriterias)
        {
            if (termCriteria.Value.Count() == 1)
            {
                filterQuery.Add(new Dictionary<string, object>() { { "term",
                    new Dictionary<string, object>() { { termCriteria.Key, termCriteria.Value.First() } } } }
                );
            }
            else
            {
                filterQuery.Add(new Dictionary<string, object>() { { "terms",
                    new Dictionary<string, object>() { { termCriteria.Key, termCriteria.Value } } } }
                );
            }
        }

        return cleanedQuery;
    }

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
                foreach (var queryExcludeField in queryExcludeFields)
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
    public static void UpdateQuery(this IDictionary<string, object>? bodyDictionary, IEnumerable<int> defaultProviderIds)
    {
        if (bodyDictionary == null)
        {
            return;
        }

        IDictionary<string, object> queryDictionary = new Dictionary<string, object>();
        IDictionary<string, object> boolDictionary = new Dictionary<string, object>();
        ICollection<IDictionary<string, object>> filterList = new List<IDictionary<string, object>>();
        ICollection<IDictionary<string, object>> shouldList = new List<IDictionary<string, object>>();
        if (bodyDictionary.TryGetValue("query", out var query))
        {
            queryDictionary = (IDictionary<string, object>)query;

            if (queryDictionary.TryGetValue("bool", out var @bool))
            {
                boolDictionary = (IDictionary<string, object>)@bool!;

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
                }
                else if (boolDictionary.TryGetValue("should", out var should))
                {
                    if (should.GetType().Name.Contains("List", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (var dictionary in (IEnumerable<object>)should)
                        {
                            shouldList.Add((IDictionary<string, object>)dictionary);
                        }
                    }
                    else
                    {
                        shouldList.Add((IDictionary<string, object>)should);
                    }
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

        // Try to rewrite all multiple term should to terms filter
        shouldList = TryOptimizeQuery(shouldList, filterList);

        if (!(CheckIfCriteriaExists(shouldList, "dataProviderId") || CheckIfCriteriaExists(filterList, "dataProviderId")))
        {
            filterList.Add(new Dictionary<string, object>() { { "terms",
                new Dictionary<string, object>() { { "dataProviderId", defaultProviderIds } } } }
            );
        }

        ICollection<IDictionary<string, object>> sightingTypeQuery = new List<IDictionary<string, object>>();
        // Only Artportalen observations with specified default sightingTypeSearchGroupId
        sightingTypeQuery.Add(new Dictionary<string, object>() { { "terms",
            new Dictionary<string, object>() { { "artportalenInternal.sightingTypeSearchGroupId",
                new[] { 1, 8, 16, 32, 128 } } } } }
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

        boolDictionary["filter"] = filterList;

        if (shouldList.Count == 0 && boolDictionary.ContainsKey("should"))
        { 
            boolDictionary.Remove("should");
        }
        else if (shouldList.Count != 0)
        {
            boolDictionary["should"] = shouldList;
        }
        
        queryDictionary["bool"] = boolDictionary;
        bodyDictionary["query"] = queryDictionary;
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
            ICollection<object> sortDictionaryList = null!;
            try
            {   // Asume sort is a list
                sortDictionaryList = (ICollection<object>)sort;
            }
            catch
            {
                // If sort was'nt a list, it's must be a dictionary
                sortDictionaryList = new List<object>
                {
                    (IDictionary<string, object>)sort
                };
            }
            finally
            {
                foreach (var obj in sortDictionaryList)
                {
                    var sortDictionary = obj as IDictionary<string, object>;
                    if ((sortDictionary?.Count ?? 0) == 0) continue;

                    if (sortDictionary!.ContainsKey("_id"))
                    {
                        sortDictionary.Add("event.endDate", new { order = "desc" });
                        sortDictionary.Remove("_id");
                    }
                }
                bodyDictionary["sort"] = sortDictionaryList;
            }
        }
    }
}
