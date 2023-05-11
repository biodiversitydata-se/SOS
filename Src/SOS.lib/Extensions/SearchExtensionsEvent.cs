using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using SOS.Lib.Enums;
using SOS.Lib.Enums.Artportalen;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using static SOS.Lib.Extensions.SearchExtensionsGeneric;

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
        public static ICollection<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>> ToQuery<TQueryContainer>(
            this EventSearchFilter filter) where TQueryContainer : class
        {
            var query = new List<Func<QueryContainerDescriptor<TQueryContainer>, QueryContainer>>();
            if (filter == null) return query;

            query.TryAddTermsCriteria("dataProviderId", filter.DataProviderIds);
            query.TryAddTermsCriteria("eventId", filter.EventIds);
            query.TryAddTermsCriteria("dataStewardship.datasetIdentifier", filter.DatasetIds);
            if (filter.IsPartOfDataStewardshipDataset.GetValueOrDefault(false))
            {
                query.AddExistsCriteria("dataStewardship");
            }

            return query;
        }

        public static List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>> ToExcludeQuery(this EventSearchFilter filter)
        {
            if (filter == null)
            {
                return null;
            }

            var query = new List<Func<QueryContainerDescriptor<dynamic>, QueryContainer>>();
            return query;
        }
    }
}