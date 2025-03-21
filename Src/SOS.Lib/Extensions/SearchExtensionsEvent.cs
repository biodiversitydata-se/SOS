using Elastic.Clients.Elasticsearch.QueryDsl;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Generic;

namespace SOS.Lib
{
    /// <summary>
    /// Observation specific search related extensions
    /// </summary>
    public static class SearchExtensionsEvent
    {
        /// <summary>
        ///     Create search filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToQuery<TQueryDescriptor>(
            this EventSearchFilter filter) where TQueryDescriptor : class
        {
            var queries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
            if (filter == null) return queries;

            queries.Add(q => q
                .TryAddTermsCriteria("dataProviderId", filter.DataProviderIds)
                .TryAddTermsCriteria("eventId", filter.EventIds)
                .TryAddTermsCriteria("dataStewardship.datasetIdentifier", filter.DatasetIds)
            );
           
            if (filter.IsPartOfDataStewardshipDataset.GetValueOrDefault(false))
            {
                queries.Add(q => q
                    .AddExistsCriteria("dataStewardship")
                );
            }

            return queries;
        }

        public static ICollection<Action<QueryDescriptor<TQueryDescriptor>>> ToExcludeQuery<TQueryDescriptor>(this EventSearchFilter filter) where TQueryDescriptor : class
        {
            if (filter == null)
            {
                return null;
            }

            var queries = new List<Action<QueryDescriptor<TQueryDescriptor>>>();
            return queries;
        }
    }
}