using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class ProcessedObservationRepository : BaseRepository<ProcessedObservation, string>, IProcessedObservationRepository
    {
        private readonly IElasticClient _elasticClient;
        private readonly int _batchSize;
        private readonly string _indexName;
        private const string ScrollTimeOut = "45s";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="exportClient"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IExportClient exportClient,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedObservationRepository> logger) : base(exportClient, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _batchSize = elasticConfiguration.BatchSize;
            _indexName = string.IsNullOrEmpty(elasticConfiguration.IndexPrefix) ?
                $"{ CollectionName.ToLower() }" :
                $"{ elasticConfiguration.IndexPrefix.ToLower() }-{ CollectionName.ToLower() }";
        }

        public async Task<ScrollResult<ProcessedProject>> ScrollProjectParametersAsync(
            FilterBase filter,
            string scrollId)
        {
            ISearchResponse<dynamic> searchResponse;
            if (string.IsNullOrEmpty(scrollId))
            {
                searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                    .Index(_indexName)
                    .Source(s => s
                        .Includes(i => i
                            .Field("projects")))
                    .Query(q => q
                        .Bool(b => b
                            .Filter(filter.ToProjectParameterQuery())
                        )
                    )
                    .Scroll(ScrollTimeOut)
                    .Size(_batchSize)
                );

            }
            else
            {
                searchResponse = await _elasticClient
                    .ScrollAsync<dynamic>(ScrollTimeOut, scrollId);
            }

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return new ScrollResult<ProcessedProject>
            {
                Records = searchResponse.Documents
                    .Select(po => (ProcessedObservation)JsonConvert.DeserializeObject<ProcessedObservation>(JsonConvert.SerializeObject(po)))
                    .SelectMany(p => p.Projects),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }

        /// <inheritdoc />
        public async Task<ScrollResult<ProcessedObservation>> ScrollObservationsAsync(FilterBase filter, string scrollId)
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

                searchResponse = await _elasticClient
                    .SearchAsync<dynamic>(s => s
                        .Index(_indexName)
                        .Source(p => projection)
                       /* .Query(q => q
                            .Bool(b => b
                                .Filter(filter.ToQuery())
                            )
                        )*/
                        .Scroll(ScrollTimeOut)
                        .Size(_batchSize)
                    );
            }
            else
            {
                searchResponse = await _elasticClient
                    .ScrollAsync<dynamic>(ScrollTimeOut, scrollId);
            }

            return new ScrollResult<ProcessedObservation>
            {
                Records = searchResponse.Documents
                    .Select(po =>
                        (ProcessedObservation)JsonConvert.DeserializeObject<ProcessedObservation>(
                            JsonConvert.SerializeObject(po))),
                ScrollId = searchResponse.ScrollId,
                TotalCount = searchResponse.HitsMetadata.Total.Value
            };
        }
    }
}
