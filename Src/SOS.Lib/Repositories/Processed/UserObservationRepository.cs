using Elastic.Clients.Elasticsearch.Cluster;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     User observation repository.
    /// </summary>
    public class UserObservationRepository : ProcessRepositoryBase<UserObservation, long>,
        IUserObservationRepository
    {
        /// <summary>
        /// Add the collection
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync()
        {
            var createIndexResponse = await Client.Indices.CreateAsync<UserObservation>(IndexName, s => s
               .Settings(s => s
                   .NumberOfShards(NumberOfShards)
                   .NumberOfReplicas(NumberOfReplicas)
                   .Settings(s => s
                       .MaxResultWindow(100000)
                       .MaxTermsCount(110000)
                   )
               )
               .Mappings(map => map
                    .Properties(ps => ps
                        .KeywordVal(kwlc => kwlc.ProvinceFeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordVal(kwlc => kwlc.MunicipalityFeatureId, IndexSetting.SearchSortAggregate)
                        .KeywordVal(kwlc => kwlc.CountryRegionFeatureId, IndexSetting.SearchSortAggregate)
                    )
                )
            );
            return createIndexResponse.Acknowledged && createIndexResponse.IsValidResponse ? true : throw new Exception($"Failed to create user observation index. Error: {createIndexResponse.DebugInformation}");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="clusterHealthCache"></param>
        /// <param name="logger"></param>
        public UserObservationRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            IClassCache<ConcurrentDictionary<string, HealthResponse>> clusterHealthCache,
            ILogger<UserObservationRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, clusterHealthCache, logger)
        {
            LiveMode = false;
            _id = nameof(Observation); // The active instance should be the same as the ProcessedObservationRepository which uses the Observation type.
        }


        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
        }

        /// <inheritdoc />
        public string UniqueIndexName => IndexHelper.GetIndexName<UserObservation>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

        /// <inheritdoc />
        public async Task<bool> VerifyCollectionAsync()
        {
            var response = await Client.Indices.ExistsAsync(IndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync();
            }

            return !response.Exists;
        }
    }
}