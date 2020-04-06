using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories.Interfaces;
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
        private const string ScrollTimeOut = "45s";

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="exportClient"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IExportClient exportClient,
            ILogger<ProcessedObservationRepository> logger) : base(exportClient, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            _batchSize = exportClient.BatchSize;
        }

        public async Task<IEnumerable<ProcessedProject>> GetProjectParameters(
            FilterBase filter, 
            int skip,
            int take)
        {
            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(CollectionName.ToLower())
                .Source(s => s
                    .Includes(i => i
                        .Field("projects")))
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .Filter(filter.ToProjectParameterQuery()))));

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

           return searchResponse.Documents
                .Select(po => (ProcessedObservation)JsonConvert.DeserializeObject<ProcessedObservation>(JsonConvert.SerializeObject(po)))
                .SelectMany(p => p.Projects);
        }

        /// <inheritdoc />
        public async Task<ScrollResult<ProcessedObservation>> ScrollAsync(FilterBase filter, string scrollId)
        {
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

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
                        .Index(CollectionName.ToLower())
                        .Source(p => projection)
                        .Query(q => q
                            .Bool(b => b
                                .Filter(filter.ToQuery())
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
