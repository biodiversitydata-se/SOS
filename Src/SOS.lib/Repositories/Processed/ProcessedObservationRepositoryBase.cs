using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessedObservationRepositoryBase : ProcessRepositoryBase<Observation>,
        IProcessedObservationRepositoryBase
    {
        private const string ScrollTimeOut = "60s";
        private readonly string _indexPrefix;
        private readonly int _scrollBatchSize;
        private readonly int _numberOfShards;
        private readonly int _numberOfReplicas;
        private readonly bool _protected;

        private async Task<bool> AddCollectionAsync()
        {
            var createIndexResponse = await ElasticClient.Indices.CreateAsync(IndexName, s => s
                .IncludeTypeName(false)
                .Settings(s => s
                    .NumberOfShards(_numberOfShards)
                    .NumberOfReplicas(_numberOfReplicas)
                    .Setting("max_terms_count", 110000)
                )
                .Map<Observation>(p => p
                    .AutoMap()
                    .Properties(ps => ps
                        .GeoShape(gs => gs
                            .Name(nn => nn.Location.Point))
                        .GeoPoint(gp => gp
                            .Name(nn => nn.Location.PointLocation))
                        .GeoShape(gs => gs
                            .Name(nn => nn.Location.PointWithBuffer)))));

            return createIndexResponse.Acknowledged && createIndexResponse.IsValid;
        }

        private Observation CastDynamicToObservation(dynamic dynamicObject)
        {
            return dynamicObject == null ? null : JsonSerializer.Deserialize<Observation>(JsonSerializer.Serialize(dynamicObject),
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
        }

        /// <summary>
        /// Delete collection
        /// </summary>
        /// <returns></returns>
        private async Task<bool> DeleteCollectionAsync()
        {
            var res = await ElasticClient.Indices.DeleteAsync(IndexName);
            return res.IsValid;
        }

        /// <summary>
        ///     Write data to elastic search
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private BulkAllObserver WriteToElastic(IEnumerable<Observation> items)
        {
            if (!items.Any())
            {
                return null;
            }

            //check
            var currentAllocation = ElasticClient.Cat.Allocation();
            if (currentAllocation != null && currentAllocation.IsValid)
            {
                string diskUsageDescription = "Current diskusage in cluster:";
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
            return ElasticClient.BulkAll(items, b => b
                    .Index(IndexName)
                    // how long to wait between retries
                    .BackOffTime("30s")
                    // how many retries are attempted if a failure occurs                        .
                    .BackOffRetries(2)
                    // how many concurrent bulk requests to make
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    // number of items per bulk request
                    .Size(1000)
                    .DroppedDocumentCallback((r, o) =>
                    {
                        Logger.LogError(r.Error.Reason);
                    })
                )
                .Wait(TimeSpan.FromDays(1),
                    next => { Logger.LogDebug($"Indexing item for search:{count += next.Items.Count}"); });
        }

        private async Task<ScrollResult<Observation>> ScrollObservationsAsync(int dataProviderId,
            string scrollId)
        {
            ISearchResponse<Observation> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await ElasticClient
                    .SearchAsync<Observation>(s => s
                        .Index(IndexName)
                        .Query(query => query.Term(term => term.Field(obs => obs.DataProviderId).Value(dataProviderId)))
                        .Scroll(ScrollTimeOut)
                        .Size(_scrollBatchSize)
                    );
            }
            else
            {
                searchResponse = await ElasticClient
                    .ScrollAsync<Observation>(ScrollTimeOut, scrollId);
            }

            return new ScrollResult<Observation>
            {
                Records = searchResponse.Documents,
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        protected readonly IElasticClient ElasticClient;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protectedIndex"></param>
        /// <param name="client"></param>
        /// <param name="elasticClient"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepositoryBase(
            bool protectedIndex,
            IProcessClient client,
            IElasticClient elasticClient,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedPublicObservationRepository> logger
        ) : base(client, true, logger)
        {
            _protected = protectedIndex;
            ElasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _indexPrefix = elasticConfiguration?.IndexPrefix ?? throw new ArgumentNullException(nameof(elasticConfiguration));
            _scrollBatchSize = client.ReadBatchSize;
            _numberOfReplicas = elasticConfiguration.NumberOfReplicas;
            _numberOfShards = elasticConfiguration.NumberOfShards;
            WriteBatchSize = elasticConfiguration.WriteBatchSize;
        }


        /// <summary>
        /// Get name of index
        /// </summary>
        public string IndexName => GetIndexName(CurrentInstance);

        /// <inheritdoc />
        public new async Task<int> AddManyAsync(IEnumerable<Observation> items)
        {
            // Save valid processed data
            Logger.LogDebug($"Start indexing batch for searching with {items.Count()} items");
            var indexResult = WriteToElastic(items);
            Logger.LogDebug("Finished indexing batch for searching");
            if (indexResult == null || indexResult.TotalNumberOfFailedBuffers > 0) return 0;
            return items.Count();
        }

        /// <inheritdoc />
        public async Task<bool> ClearCollectionAsync()
        {
            await DeleteCollectionAsync();
            return await AddCollectionAsync();
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
        public async Task<bool> DeleteByOccurrenceIdAsync(IEnumerable<string> occurenceIds)
        {
            try
            {
                // Create the collection
                var res = await ElasticClient.DeleteByQueryAsync<Observation>(q => q
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
                var res = await ElasticClient.DeleteByQueryAsync<Observation>(q => q
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
        public async Task<bool> DeleteArtportalenBatchAsync(ICollection<int> sightingIds)
        {
            try
            {
                // Create the collection
                var res = await ElasticClient.DeleteByQueryAsync<Observation>(q => q
                    .Index(IndexName)
                    .Query(q => q
                        .Terms(t => t
                            .Field(f => f.ArtportalenInternal.SightingId)
                            .Terms(sightingIds)
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
        public async Task<bool> DisableIndexingAsync()
        {
            var updateSettingsResponse =
                await ElasticClient.Indices.UpdateSettingsAsync(IndexName,
                    p => p.IndexSettings(g => g.RefreshInterval(-1)));

            return updateSettingsResponse.Acknowledged && updateSettingsResponse.IsValid;
        }

        /// <inheritdoc />
        public async Task EnableIndexingAsync()
        {
            await ElasticClient.Indices.UpdateSettingsAsync(IndexName,
                p => p.IndexSettings(g => g.RefreshInterval(1)));
        }

        /// <inheritdoc />
        public string GetIndexName(byte instance) => IndexHelper.GetIndexName<Observation>(_indexPrefix, true, instance, _protected);

        /// <inheritdoc />
        public async Task<DateTime> GetLatestModifiedDateForProviderAsync(int providerId)
        {
            try
            {
                var res = await ElasticClient.SearchAsync<Observation>(s => s
                    .Index(IndexName)
                    .Query(q => q
                        .Term(t => t
                            .Field(f => f.DataProviderId)
                            .Value(providerId)))
                    .Aggregations(a => a
                        .Max("latestModified", m => m
                            .Field(f => f.Modified)
                        )
                    )
                );

                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                return epoch.AddMilliseconds(res.Aggregations?.Max("latestModified")?.Value ?? 0).ToUniversalTime();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to get last modified date for provider: { providerId }, index: { IndexName }");
                return DateTime.MinValue;
            }
        }

        /// <inheritdoc />
        public async Task<long> IndexCount()
        {
            try
            {
                var countResponse = await ElasticClient.CountAsync<Observation>(s => s
                    .Index(IndexName)
                );

                if (!countResponse.IsValid)
                {
                    throw new InvalidOperationException(countResponse.DebugInformation);
                }

                return countResponse.Count;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return -1;
            }
        }

        /// <inheritdoc />
        public async Task<ScrollResult<SimpleMultimediaRow>> ScrollMultimediaAsync(
            FilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await ElasticClient.SearchAsync<dynamic>(s => s
                    .Index(IndexName)
                    .Source(source => source
                        .Includes(fieldsDescriptor => fieldsDescriptor
                            .Field("occurrence.occurrenceId")
                            .Field("media")))
                    .Query(query => query
                        .Bool(boolQueryDescriptor => boolQueryDescriptor
                            .Filter(filter.ToMultimediaQuery())
                        )
                    )
                    .Scroll(ScrollTimeOut)
                    .Size(BatchSize)
                );
            }
            else
            {
                searchResponse = await ElasticClient
                    .ScrollAsync<dynamic>(ScrollTimeOut, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);


            return new ScrollResult<SimpleMultimediaRow>
            {
                Records = searchResponse.Documents.Select(d => (Observation)CastDynamicToObservation(d))?.ToSimpleMultimediaRows(),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<ExtendedMeasurementOrFactRow>> ScrollMeasurementOrFactsAsync(
            FilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await ElasticClient.SearchAsync<dynamic>(s => s
                    .Index(IndexName)
                    .Source(source => source
                        .Includes(fieldsDescriptor => fieldsDescriptor
                            .Field("occurrence.occurrenceId")
                            .Field("measurementOrFacts")))
                    .Query(query => query
                        .Bool(boolQueryDescriptor => boolQueryDescriptor
                            .Filter(filter.ToMeasurementOrFactsQuery())
                        )
                    )
                    .Scroll(ScrollTimeOut)
                    .Size(BatchSize)
                );
            }
            else
            {
                searchResponse = await ElasticClient
                    .ScrollAsync<Observation>(ScrollTimeOut, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new ScrollResult<ExtendedMeasurementOrFactRow>
            {
                Records = searchResponse.Documents.Select(d => (Observation)CastDynamicToObservation(d))?.ToExtendedMeasurementOrFactRows(),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<Observation>> TypedScrollObservationsAsync(
            FilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;

            if (string.IsNullOrEmpty(scrollId))
            {
                var query = filter.ToQuery();
                var projection = new SourceFilterDescriptor<dynamic>()
                    .Excludes(e => e
                        .Field("artportalenInternal")
                        .Field("location.point")
                        .Field("location.pointLocation")
                        .Field("location.pointWithBuffer")
                    );
               
                searchResponse = await ElasticClient
                    .SearchAsync<dynamic>(s => s
                        .Index(IndexName)
                        .Source(p => projection)
                        .Query(q => q
                            .Bool(b => b
                                .Filter(query)
                            )
                        )
                        .Scroll(ScrollTimeOut)
                        .Size(1000)
                    );

            }
            else
            {
                searchResponse = await ElasticClient
                    .ScrollAsync<Observation>(ScrollTimeOut, scrollId);
            }

            return new ScrollResult<Observation>
            {
                Records = searchResponse.Documents.Select(d => (Observation)CastDynamicToObservation(d)),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata?.Total?.Value ?? 0
            };
        }


        /// <inheritdoc />
        public async Task<ScrollResult<Observation>> ScrollObservationsAsync(FilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                var projection = new SourceFilterDescriptor<dynamic>()
                    .Excludes(e => e
                        .Field("location.point")
                        .Field("location.pointLocation")
                        .Field("location.pointWithBuffer")
                    );

                searchResponse = await ElasticClient
                    .SearchAsync<dynamic>(s => s
                        .Index(IndexName)
                        .Source(p => projection)
                        /* .Query(q => q
                             .Bool(b => b
                                 .Filter(filter.ToQuery())
                             )
                         )*/
                        .Scroll(ScrollTimeOut)
                        .Size(BatchSize)
                    );
            }
            else
            {
                searchResponse = await ElasticClient
                    .ScrollAsync<dynamic>(ScrollTimeOut, scrollId);
            }

            return new ScrollResult<Observation>
            {
                Records = searchResponse.Documents
                    .Select(po =>
                        (Observation)JsonConvert.DeserializeObject<Observation>(
                            JsonConvert.SerializeObject(po))),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<bool> ValidateProtectionLevelAsync()
        {
            try
            {
                var countResponse = await ElasticClient.CountAsync<Observation>(s => s
                    .Index(IndexName)
                    .Query(q => q
                        .Term(t => t.Field(f => f.Protected).Value(!_protected))
                    )
                );

                if (!countResponse.IsValid)
                {
                    throw new InvalidOperationException(countResponse.DebugInformation);
                }
                if (!countResponse.Count.Equals(0))
                {
                    Logger.LogError($"Failed to validate protection level for Index: {IndexName}, count of observations with protection:{_protected} = {countResponse.Count}, should be 0");
                }
                return countResponse.Count.Equals(0);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> VerifyCollectionAsync()
        {
            var response = await ElasticClient.Indices.ExistsAsync(IndexName);

            if (!response.Exists)
            {
                await AddCollectionAsync();
            }

            return !response.Exists;
        }

        /// <inheritdoc />
        public int WriteBatchSize { get; set; }
    }
}