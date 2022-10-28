using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Dataset;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Repositories.Processed.Interfaces;
using static SOS.Lib.Models.Processed.Dataset.ObservationDataset;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Dataset observation repository.
    /// </summary>
    public class ObservationDatasetRepository : ProcessRepositoryBase<ObservationDataset, string>,
        IObservationDatasetRepository
    {
        /// <summary>
        /// Add the collection
        /// </summary>
        /// <returns></returns>
        private async Task<bool> AddCollectionAsync()
        {
            var createIndexResponse = await Client.Indices.CreateAsync(IndexName, s => s
                .Settings(s => s
                    .NumberOfShards(NumberOfShards)
                    .NumberOfReplicas(NumberOfReplicas)
                    .Setting("max_terms_count", 110000)
                    .Setting(UpdatableIndexSettings.MaxResultWindow, 100000)
                )
                .Map<ObservationDataset>(m => m
                    .AutoMap<ObservationDataset>()
                    .Properties(ps => ps
                        .KeyWordLowerCase(kwlc => kwlc.Id)
                        .KeyWordLowerCase(kwlc => kwlc.Identifier)                        
                        .KeyWordLowerCase(kwlc => kwlc.DataStewardship)
                        .KeyWordLowerCase(kwlc => kwlc.Title, false)
                        .KeyWordLowerCase(kwlc => kwlc.ProjectId)
                        .KeyWordLowerCase(kwlc => kwlc.ProjectCode, false)
                        .KeyWordLowerCase(kwlc => kwlc.Description, false)
                        .KeyWordLowerCase(kwlc => kwlc.Spatial, false)
                        .KeyWordLowerCase(kwlc => kwlc.Language, false)
                        .KeyWordLowerCase(kwlc => kwlc.Metadatalanguage, false)
                        .Object<Organisation>(t => t
                            .AutoMap()
                            .Name(nm => nm.Assigner)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationCode, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationID, false)
                            )
                        )
                        .Object<Organisation>(t => t
                            .AutoMap()
                            .Name(nm => nm.Creator)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationCode, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationID, false)
                            )
                        )
                        .Object<Organisation>(t => t
                            .AutoMap()
                            .Name(nm => nm.OwnerinstitutionCode)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationCode, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationID, false)
                            )
                        )
                        .Object<Organisation>(t => t
                            .AutoMap()
                            .Name(nm => nm.Publisher)
                            .Properties(ps => ps
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationCode, false)
                                .KeyWordLowerCase(kwlc => kwlc.OrganisationID, false)
                            )
                        )
                    )
                )
            );
            
            return createIndexResponse.Acknowledged && createIndexResponse.IsValid ? true : throw new Exception($"Failed to create ObservationDataset index. Error: {createIndexResponse.DebugInformation}");
        }

        public async Task<List<ObservationDataset>> GetDatasetsByIds(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any()) throw new ArgumentException("ids is empty");

            var query = new List<Func<QueryContainerDescriptor<ObservationDataset>, QueryContainer>>();
            query.TryAddTermsCriteria("identifier", ids);
            var searchResponse = await Client.SearchAsync<ObservationDataset>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                    )
                )
                .Size(ids?.Count() ?? 0)                
                .TrackTotalHits(false)
            );

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);
            var datasets = searchResponse.Documents.ToList();
            return datasets;
        }

        /// <summary>
        /// Delete collection
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteCollectionAsync()
        {
            var res = await Client.Indices.DeleteAsync(IndexName);
            return res.IsValid;
        }

        /// <summary>
        /// Write data to elastic search
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private BulkAllObserver WriteToElastic(IEnumerable<ObservationDataset> items)
        {
            if (!items.Any())
            {
                return null;
            }

            //check
            var currentAllocation = Client.Cat.Allocation();
            if (currentAllocation != null && currentAllocation.IsValid)
            {
                var diskUsageDescription = "Current diskusage in cluster:";
                foreach (var record in currentAllocation.Records)
                {
                    if (int.TryParse(record.DiskPercent, out int percentageUsed))
                    {
                        diskUsageDescription += percentageUsed + "% ";
                        if (percentageUsed > 90)
                        {
                            Logger.LogError($"Disk usage too high in cluster ({percentageUsed}%), aborting indexing");
                            return null;
                        }
                    }
                }

                Logger.LogDebug(diskUsageDescription);
            }

            var count = 0;
            return Client.BulkAll(items, b => b
                    .Index(IndexName)
                    // how long to wait between retries
                    .BackOffTime("30s")
                    // how many retries are attempted if a failure occurs                        .
                    .BackOffRetries(2)
                    // how many concurrent bulk requests to make
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    // number of items per bulk request
                    .Size(WriteBatchSize)
                    .DroppedDocumentCallback((r, o) =>
                    {
                        Logger.LogError($"Dataset id: {o?.Id}, Error: {r?.Error?.Reason}");
                    })
                )
                .Wait(TimeSpan.FromDays(1),
                    next => { Logger.LogDebug($"Indexing datasets for search:{count += next.Items.Count}"); });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        public ObservationDatasetRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ObservationDatasetRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = true;
            _id = nameof(Observation); // The active instance should be the same as the ProcessedObservationRepository which uses the Observation type.
        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<ObservationDataset> items)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing ObservationDataset batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items);
            Logger.LogDebug("Finished indexing ObservationDataset batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        public async Task<bool> DeleteAllDocumentsAsync()
        {
            try
            {
                var res = await Client.DeleteByQueryAsync<ObservationDataset>(q => q
                    .Index(IndexName)
                    .Query(q => q.MatchAll())
                );

                return res.IsValid;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
        }

        /// <inheritdoc />
        public async Task<bool> DisableIndexingAsync()
        {
            var updateSettingsResponse =
                await Client.Indices.UpdateSettingsAsync(IndexName,
                    p => p.IndexSettings(g => g.RefreshInterval(-1)));

            return updateSettingsResponse.Acknowledged && updateSettingsResponse.IsValid;
        }

        /// <inheritdoc />
        public async Task EnableIndexingAsync()
        {
            await Client.Indices.UpdateSettingsAsync(IndexName,
                p => p.IndexSettings(g => g.RefreshInterval(1)));
        }

        /// <inheritdoc />
        public string UniqueIndexName => IndexHelper.GetIndexName<ObservationDataset>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

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