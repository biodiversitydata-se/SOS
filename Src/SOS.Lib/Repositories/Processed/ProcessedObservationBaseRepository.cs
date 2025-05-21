using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public abstract class ProcessedObservationBaseRepository : ProcessRepositoryBase<Observation, string>
    {
        /// <summary>
        /// Get core queries
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skipAuthorizationFilters"></param>
        /// <returns></returns>
        protected (ICollection<Action<QueryDescriptor<TQueryDescriptor>>> Filter, ICollection<Action<QueryDescriptor<TQueryDescriptor>>> Exclude)
            GetCoreQueries<TQueryDescriptor>(SearchFilterBase filter, bool skipAuthorizationFilters = false) where TQueryDescriptor : class
        {
            var queries = filter.ToQuery<TQueryDescriptor>(skipAuthorizationFilters: skipAuthorizationFilters);
            var excludeQueries = filter.ToExcludeQuery<TQueryDescriptor>();

            return (queries, excludeQueries);
        }

        /// <summary>
        /// Get public index name and also protected index name if user is authorized
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skipAuthorizationFilters"></param>
        /// <returns></returns>
        protected string GetCurrentIndex(SearchFilterBase filter, bool skipAuthorizationFilters = false)
        {
            if (
                (
                    (filter.ExtendedAuthorization.ObservedByMe ?? false) ||
                    (filter.ExtendedAuthorization.ReportedByMe ?? false) ||
                    (!filter.ExtendedAuthorization.ProtectionFilter.Equals(ProtectionFilter.Public))
                ) &&
                (filter?.ExtendedAuthorization.UserId ?? 0) == 0
            )
            {
                if (!skipAuthorizationFilters) throw new AuthenticationRequiredException("Not authenticated");
            }

            return filter.ExtendedAuthorization.ProtectionFilter switch
            {
                ProtectionFilter.BothPublicAndSensitive => $"{PublicIndexName}, {ProtectedIndexName}",
                ProtectionFilter.Sensitive => ProtectedIndexName,
                _ => PublicIndexName
            };
        }
        
        /// <summary>
        /// Constructor used in admin mode
        /// </summary>
        /// <param name="liveMode"></param>
        /// <param name="elasticClientManager"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="clusterHealthCache"></param>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        public ProcessedObservationBaseRepository(
            bool liveMode,
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ElasticSearchConfiguration elasticConfiguration,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            IMemoryCache memoryCache,
            ILogger<ProcessedObservationBaseRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, memoryCache, logger)
        {
            LiveMode = liveMode;
        }

        /// <inheritdoc />
        public string PublicIndexName => IndexName;

        /// <inheritdoc />
        public string ProtectedIndexName => IndexHelper.GetIndexName<Observation>(IndexPrefix, ClientCount == 1, LiveMode ? ActiveInstance : InActiveInstance, true);
    }
}
