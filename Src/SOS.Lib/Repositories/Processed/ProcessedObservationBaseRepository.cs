using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        protected (ICollection<Action<QueryDescriptor<dynamic>>> Filter, ICollection<Action<QueryDescriptor<dynamic>>> Exclude)
            GetCoreQueries(SearchFilterBase filter, bool skipAuthorizationFilters = false)
        {
            var query = filter.ToQuery<dynamic>(skipAuthorizationFilters: skipAuthorizationFilters);
            var excludeQuery = filter.ToExcludeQuery<dynamic>();

            return (query, excludeQuery);
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
        /// Execute search after query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="searchIndex"></param>
        /// <param name="searchDescriptor"></param>
        /// <param name="pointInTimeId"></param>
        /// <param name="nextPageKey"></param>
        /// <returns></returns>
        protected async Task<SearchResponse<T>> SearchAfterAsync<T>(
           string searchIndex,
           SearchRequestDescriptor<T> searchDescriptor,
           string pointInTimeId = null,
           ICollection<FieldValue> nextPageKey = null) where T : class
        {
            var keepAlive = "10m";
            if (string.IsNullOrEmpty(pointInTimeId))
            {
                var pitResponse = await Client.OpenPointInTimeAsync(searchIndex, pit => pit
                    .RequestConfiguration(c => c
                        .RequestTimeout(TimeSpan.FromSeconds(30))
                    )
                    .KeepAlive(keepAlive)
                );
                pointInTimeId = pitResponse.Id;
            }

            // Retry policy by Polly
            var searchResponse = await PollyHelper.GetRetryPolicy(3, 100).ExecuteAsync(async () =>
            {
                var queryResponse = await Client.SearchAsync(searchDescriptor
                   .Index(searchIndex)
                   .Sort(s => s.Field("_shard_doc"))
                   .SearchAfter(nextPageKey)
                   .Size(ScrollBatchSize)
                   .TrackTotalHits(new Elastic.Clients.Elasticsearch.Core.Search.TrackHits(false))
                   .Pit(pointInTimeId, pit => pit.KeepAlive(keepAlive))
                );

                queryResponse.ThrowIfInvalid();

                return queryResponse;
            });

            if (!string.IsNullOrEmpty(pointInTimeId) && (searchResponse?.Hits?.Count ?? 0) == 0)
            {
                await Client.ClosePointInTimeAsync(pitr => pitr.Id(pointInTimeId));
            }

            return searchResponse;
        }

        /// <summary>
        /// Constructor used in admin mode
        /// </summary>
        /// <param name="liveMode"></param>
        /// <param name="elasticClientManager"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="clusterHealthCache"></param>
        /// <param name="logger"></param>
        public ProcessedObservationBaseRepository(
            bool liveMode,
            IElasticClientManager elasticClientManager,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ElasticSearchConfiguration elasticConfiguration,
             IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            ILogger<ProcessedObservationBaseRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, logger)
        {
            LiveMode = liveMode;
        }

        /// <inheritdoc />
        public string PublicIndexName => IndexName;

        /// <inheritdoc />
        public string ProtectedIndexName => IndexHelper.GetIndexName<Observation>(IndexPrefix, ClientCount == 1, LiveMode ? ActiveInstance : InActiveInstance, true);
    }
}
