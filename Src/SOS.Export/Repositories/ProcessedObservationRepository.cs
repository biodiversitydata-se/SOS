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

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="elasticClient"></param>
        /// <param name="exportClient"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ProcessedObservationRepository(
            IElasticClient elasticClient,
            IExportClient exportClient,
            ILogger<ProcessedObservationRepository> logger) : base(exportClient, true, logger)
        {
            _elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProcessedObservation>> GetChunkAsync(FilterBase filter, int skip, int take)
        {
            if (!filter?.IsFilterActive ?? true)
            {
                return null;
            }

            var projection = new SourceFilterDescriptor<dynamic>()
                .Excludes(e => e
                    .Field("location.point")
                    .Field("location.pointLocation")
                    .Field("location.pointWithBuffer")
                );
            var x = new SourceFilter();

            var searchResponse = await _elasticClient.SearchAsync<dynamic>(s => s
                .Index(CollectionName.ToLower())
                .Source(new string[0].ToProjection())
                .From(skip)
                .Size(take)
                .Query(q => q
                    .Bool(b => b
                        .Filter(filter.ToQuery()))));

            if (!searchResponse.IsValid) throw new InvalidOperationException(searchResponse.DebugInformation);

            return (IEnumerable<ProcessedObservation>)searchResponse.Documents;
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
    }
}
