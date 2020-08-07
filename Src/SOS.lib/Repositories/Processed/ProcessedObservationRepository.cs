using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessedObservationRepository : ProcessRepositoryBase<ProcessedObservation>,
        IProcessedObservationRepository
    {
        private const string ScrollTimeOut = "45s";
        private readonly IElasticClient _elasticClient;
        private readonly string _indexPrefix;
        private readonly int _scrollBatchSize;

        private async Task<bool> AddCollectionAsync()
        {
            var createIndexResponse = await _elasticClient.Indices.CreateAsync(IndexName, s => s
                .IncludeTypeName(false)
                .Settings(s => s
                    .NumberOfShards(6)
                    .NumberOfReplicas(0)
                )
                .Map<ProcessedObservation>(p => p
                    .AutoMap()
                    .Properties(ps => ps
                        .GeoShape(gs => gs
                            .Name(nn => nn.Location.Point))
                        .GeoPoint(gp => gp
                            .Name(nn => nn.Location.PointLocation))
                        .GeoShape(gs => gs
                            .Name(nn => nn.Location.PointWithBuffer)))));

            if (createIndexResponse.Acknowledged && createIndexResponse.IsValid)
            {
                var updateSettingsResponse =
                    await _elasticClient.Indices.UpdateSettingsAsync(IndexName,
                        p => p.IndexSettings(g => g.RefreshInterval(-1)));

                return updateSettingsResponse.Acknowledged && updateSettingsResponse.IsValid;
            }

            return false;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="elasticClient"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IProcessClient client,
            IElasticClient elasticClient,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedObservationRepository> logger
        ) : base(client, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _indexPrefix = elasticConfiguration.IndexPrefix;
            _scrollBatchSize = client.ReadBatchSize; 
        }

        public string IndexName => string.IsNullOrEmpty(_indexPrefix)
            ? $"{CurrentInstanceName.ToLower()}"
            : $"{_indexPrefix.ToLower()}-{CurrentInstanceName.ToLower()}";

        
        /// <inheritdoc />
        public new async Task<int> AddManyAsync(IEnumerable<ProcessedObservation> items)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items);
            Logger.LogDebug("Finished indexing batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        /// <inheritdoc />
        public async Task CreateIndexAsync()
        {
            await _elasticClient.Indices.UpdateSettingsAsync(IndexName,
                p => p.IndexSettings(g => g.RefreshInterval(1)));
        }

        /// <inheritdoc />
        public async Task<bool> CopyProviderDataAsync(DataProvider dataProvider)
        {
            var scrollResult = await ScrollObservationsAsync(dataProvider.Id, null);
            var success = true;

            while (scrollResult?.Records?.Any() ?? false)
            {
                var processedObservations = scrollResult.Records;
                var indexResult = WriteToElastic(processedObservations);
                if (indexResult.TotalNumberOfFailedBuffers != 0) success = false;
                scrollResult = await ScrollObservationsAsync(dataProvider.Id, scrollResult.ScrollId);
            }

            return success;
        }

        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
        }

        public async Task<bool> DeleteCollectionAsync()
        {
            var res = await _elasticClient.Indices.DeleteAsync(IndexName);
            return res.IsValid;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteByOccurenceIdAsync(IEnumerable<string> occurenceIds)
        {
            try
            {
                // Create the collection
                var res = await _elasticClient.DeleteByQueryAsync<ProcessedObservation>(q => q
                    .Index(IndexName)
                    .Query(q => q
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .Terms(occurenceIds)
                        )
                    )
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
        public async Task<bool> DeleteProviderDataAsync(DataProvider dataProvider)
        {
            try
            {
                // Create the collection
                var res = await _elasticClient.DeleteByQueryAsync<ProcessedObservation>(q => q
                    .Index(IndexName)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.DataProviderId)
                            .Value(dataProvider.Id))));

                return res.IsValid;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<int> GetMaxIdForProviderAsync(int providerId)
        {
            try
            {
                // Create the collection
                var res = await _elasticClient.SearchAsync<ProcessedObservation>(s => s
                    .Index(IndexName)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.DataProviderId)
                            .Value(providerId)))
                    .Aggregations(a => a
                        .Max("maxId", m => m
                        .Field(f => f.VerbatimId)
                        )
                     )
                );

                return (int)res.Aggregations.Max("maxId").Value;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<bool> VerifyCollectionAsync()
        {
            var response = await _elasticClient.Indices.ExistsAsync(IndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync();
            }

            return !response.Exists;
        }

        /// <summary>
        ///     Write data to elastic search
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private BulkAllObserver WriteToElastic(IEnumerable<ProcessedObservation> items)
        {
            if (!items.Any())
            {
                return null;
            }

            var count = 0;
            return _elasticClient.BulkAll(items, b => b
                    .Index(IndexName)
                    // how long to wait between retries
                    .BackOffTime("30s")
                    // how many retries are attempted if a failure occurs                        .
                    .BackOffRetries(2)
                    // how many concurrent bulk requests to make
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    // number of items per bulk request
                    .Size(1000)
                )
                .Wait(TimeSpan.FromDays(1),
                    next => { Logger.LogDebug($"Indexing item for search:{count += next.Items.Count}"); });
        }

        private async Task<ScrollResult<ProcessedObservation>> ScrollObservationsAsync(int dataProviderId,
            string scrollId)
        {
            ISearchResponse<ProcessedObservation> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await _elasticClient
                    .SearchAsync<ProcessedObservation>(s => s
                        .Index(IndexName)
                        .Query(query => query.Term(term => term.Field(obs => obs.DataProviderId).Value(dataProviderId)))
                        .Scroll(ScrollTimeOut)
                        .Size(_scrollBatchSize)
                    );
            }
            else
            {
                searchResponse = await _elasticClient
                    .ScrollAsync<ProcessedObservation>(ScrollTimeOut, scrollId);
            }

            return new ScrollResult<ProcessedObservation>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }
    }
}