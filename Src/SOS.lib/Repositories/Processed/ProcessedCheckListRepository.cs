using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Checklist;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class ProcessedChecklistRepository : ProcessRepositoryBase<Checklist, string>,
        IProcessedChecklistRepository
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
                .Map<Checklist>(p => p
                    .Properties(ps => ps
                        .Date(d => d
                            .Name(nm => nm.Modified)
                        )
                        .Date(d => d
                            .Name(nm => nm.RegisterDate)
                        )
                        .Keyword(k => k
                            .Name(nm => nm.Id)
                        )
                        .Keyword(k => k
                            .Name(nm => nm.Name)
                        )
                        .Keyword(k => k
                            .Name(nm => nm.OccurrenceIds)
                        )
                        .Keyword(k => k
                            .Name(nm => nm.RecordedBy)
                        )
                        .Keyword(k => k
                            .Name(nm => nm.SamplingEffortTime)
                            .Index(false)
                        )
                        .Number(no => no
                            .Name(nm => nm.DataProviderId)
                            .Type(NumberType.Integer)
                        )
                        .Number(no => no
                            .Name(nm => nm.TaxonIds)
                            .Type(NumberType.Integer)
                        )
                        .Number(no => no
                            .Name(nm => nm.TaxonIdsFound)
                            .Type(NumberType.Integer)
                        )
                        .Object<ApInternal>(ai => ai
                            .AutoMap()
                            .Name(nm => nm.ArtportalenInternal)
                        )
                        .Object<Event>(t => t
                            .AutoMap()
                            .Name(nm => nm.Event)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<Location>(l => l
                            .AutoMap()
                            .Name(nm => nm.Location)
                            .Properties(ps => ps.GetMapping())
                        )
                        .Object<Project>(p => p
                            .AutoMap()
                            .Name(nm => nm.Project)
                            .Properties(ps => ps.GetMapping())
                        )
                    )
                )
            );

            return createIndexResponse.Acknowledged && createIndexResponse.IsValid ? true : throw new Exception($"Failed to create checklist index. Error: {createIndexResponse.DebugInformation}");
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
        private BulkAllObserver WriteToElastic(IEnumerable<Checklist> items)
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
                        Logger.LogError($"Check list id: {o?.Id}, Error: {r.Error.Reason}");
                    })
                )
                .Wait(TimeSpan.FromDays(1),
                    next => { Logger.LogDebug($"Indexing checklists for search:{count += next.Items.Count}"); });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClientManager"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="processedConfigurationCache"></param>
        /// <param name="logger"></param>
        public ProcessedChecklistRepository(
            IElasticClientManager elasticClientManager,
            ElasticSearchConfiguration elasticConfiguration,
            ICache<string, ProcessedConfiguration> processedConfigurationCache,
            ILogger<ProcessedChecklistRepository> logger) : base(true, elasticClientManager, processedConfigurationCache, elasticConfiguration, logger)
        {
            LiveMode = false;
        }

        /// <inheritdoc />
        public async Task<int> AddManyAsync(IEnumerable<Checklist> items)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing checklist batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items);
            Logger.LogDebug("Finished indexing checklist batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        public async Task<bool> DeleteAllDocumentsAsync()
        {
            try
            {
                var res = await Client.DeleteByQueryAsync<Observation>(q => q
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
        public async Task<Checklist> GetAsync(string id, bool internalCall)
        {
            var searchResponse = await Client.SearchAsync<Checklist>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f.Term(t => t
                            .Field("_id")
                            .Value(id))))
                    )
                
                .Size(1)
                .Source(s => s
                    .Excludes(e => internalCall ? null : e.Field(f => f.ArtportalenInternal))
                )
                .TrackTotalHits(false)
            );

            searchResponse.ThrowIfInvalid();

            return searchResponse.Documents?.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<PagedResult<Checklist>> GetChunkAsync(SearchFilter filter, int skip, int take, string sortBy,
            SearchSortOrder sortOrder)
        {
            var searchResponse = await Client.SearchAsync<Checklist>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f.Exists(e => e.Field(f => f.Id)))
                    )
                )
                .Sort(sort => sort.Field(f => f.Field(f => f.Name)))
                .From(skip)
                .Size(take)
            );

            searchResponse.ThrowIfInvalid();

            return new PagedResult<Checklist>
            {
                Records = searchResponse.Documents,
                Skip = skip,
                Take = take,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };

            // When operation is disposed, telemetry item is sent.
        }


        /// <summary>
        /// Count number of checklists matching the search filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<int> GetChecklistCountAsync(ChecklistSearchFilter filter)
        {
            var query = filter.ToQuery<Checklist>();            

            var countResponse = await Client.CountAsync<Checklist>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)                        
                    )
                )
            );
            countResponse.ThrowIfInvalid();

            return Convert.ToInt32(countResponse.Count);
        }

        /// <summary>
        /// Count number of present observations matching the search filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<int> GetPresentCountAsync(ChecklistSearchFilter filter)
        {
            var query = filter.ToQuery<Checklist>();
            var foundQuery = new List<Func<QueryContainerDescriptor<Checklist>, QueryContainer>>();
            foundQuery.TryAddTermsCriteria("taxonIdsFound", filter.Taxa?.Ids);

            var countResponse = await Client.CountAsync<Checklist>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                        .Must(foundQuery)
                    )
                )
            );
            countResponse.ThrowIfInvalid();

            return Convert.ToInt32(countResponse.Count);
        }               

        /// <summary>
        /// Count number of absent observations (Using taxonIdsFound property) matching the search filter.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<int> GetAbsentCountAsync(ChecklistSearchFilter filter)
        {
            var query = filter.ToQuery<Checklist>();
            var nonQuery = new List<Func<QueryContainerDescriptor<Checklist>, QueryContainer>>();
            nonQuery.TryAddTermsCriteria("taxonIdsFound", filter.Taxa?.Ids);

            var countResponse = await Client.CountAsync<Checklist>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Filter(query)
                        .MustNot(nonQuery)
                    )
                )
            );
            countResponse.ThrowIfInvalid();

            return Convert.ToInt32(countResponse.Count);
        }

        /// <inheritdoc />
        public string UniqueIndexName => IndexHelper.GetIndexName<Checklist>(IndexPrefix, true, LiveMode ? ActiveInstance : InActiveInstance, false);

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