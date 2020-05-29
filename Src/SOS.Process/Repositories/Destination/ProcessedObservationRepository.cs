using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessedObservationRepository : ProcessBaseRepository<ProcessedObservation, string>,
        IProcessedObservationRepository
    {
        private const string ScrollTimeOut = "45s";
        private readonly IElasticClient _elasticClient;
        private readonly string _indexPrefix;
        private readonly IInvalidObservationRepository _invalidObservationRepository;
        private readonly int _scrollBatchSize;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="elasticClient"></param>
        /// <param name="invalidObservationRepository"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        /// <param name="elasticClient"></param>
        public ProcessedObservationRepository(
            IProcessClient client,
            IElasticClient elasticClient,
            IInvalidObservationRepository invalidObservationRepository,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedObservationRepository> logger
        ) : base(client, true, logger)
        {
            _invalidObservationRepository = invalidObservationRepository ??
                                            throw new ArgumentNullException(nameof(invalidObservationRepository));
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));

            _indexPrefix = elasticConfiguration.IndexPrefix;
            _scrollBatchSize = client.BatchSize;
        }

        public string IndexName => string.IsNullOrEmpty(_indexPrefix)
            ? $"{_collectionName.ToLower()}"
            : $"{_indexPrefix.ToLower()}-{_collectionName.ToLower()}";

        /// <inheritdoc />
        public new async Task<int> AddManyAsync(IEnumerable<ProcessedObservation> items)
        {
            // Separate valid and invalid data
            var invalidObservations = Validate(ref items);

            // Save valid processed data
            Logger.LogDebug($"Start indexing batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items);
            Logger.LogDebug("Finished indexing batch for searching");

            if ((indexResult?.TotalNumberOfFailedBuffers ?? 0) == 0)
            {
                if (invalidObservations?.Any() ?? false)
                {
                    await _invalidObservationRepository.AddManyAsync(invalidObservations);
                }

                return items.Count();
            }

            return 0;
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

        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
        }

        public override async Task<bool> DeleteCollectionAsync()
        {
            var res = await _elasticClient.Indices.DeleteAsync(IndexName);
            return res.IsValid;
        }

        public override async Task<bool> AddCollectionAsync()
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

        /// <inheritdoc />
        public override async Task<bool> VerifyCollectionAsync()
        {
            var response = await _elasticClient.Indices.ExistsAsync(IndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync();
            }

            // Make sure invalid collection is empty 
            await _invalidObservationRepository.DeleteCollectionAsync();
            await _invalidObservationRepository.AddCollectionAsync();
            await _invalidObservationRepository.CreateIndexAsync();

            return !response.Exists;
        }

        /// <summary>
        ///     Validate Darwin core.
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Invalid items</returns>
        private IEnumerable<InvalidObservation> Validate(
            ref IEnumerable<ProcessedObservation> items)
        {
            var validItems = new List<ProcessedObservation>();
            var invalidItems = new List<InvalidObservation>();

            foreach (var item in items)
            {
                var invalidObservation =
                    new InvalidObservation(item.DatasetId, item.DatasetName, item.Occurrence.OccurrenceId);

                if (item.Taxon == null)
                {
                    invalidObservation.Defects.Add("Taxon not found");
                }

                if ((item.Location?.CoordinateUncertaintyInMeters ?? 0) > 100000)
                {
                    invalidObservation.Defects.Add("CoordinateUncertaintyInMeters exceeds max value 100 km");
                }

                if (!item.IsInEconomicZoneOfSweden)
                {
                    invalidObservation.Defects.Add("Sighting outside Swedish economic zone");
                }

                if (string.IsNullOrEmpty(item?.Occurrence.CatalogNumber))
                {
                    invalidObservation.Defects.Add("CatalogNumber is missing");
                }

                if (invalidObservation.Defects.Any())
                {
                    invalidItems.Add(invalidObservation);
                }
                else
                {
                    validItems.Add(item);
                }
            }

            items = validItems;

            return invalidItems.Any() ? invalidItems : null;
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