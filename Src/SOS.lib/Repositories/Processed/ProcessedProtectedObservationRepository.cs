using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessedProtectedObservationRepository : ProcessedObservationRepositoryBase,
        IProcessedProtectedObservationRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="elasticClient"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedProtectedObservationRepository(
            IProcessClient client,
            IElasticClient elasticClient,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedPublicObservationRepository> logger
        ) : base(true, client, elasticClient, elasticConfiguration, logger)
        {
            
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetOccurrenceIdsAsync(int noOfOccurrences)
        {
            var searchResponse = await ElasticClient.SearchAsync<Observation>(s => s
                .Index(IndexName)
                .Source(s => s.Includes(i => i.Fields(f => f.Occurrence.OccurrenceId)))
                .From(0)
                .Size(noOfOccurrences)
            );

            if (!searchResponse.IsValid)
            {
                throw new InvalidOperationException(searchResponse.DebugInformation);
            }

            return searchResponse.Documents.Select(o => o.Occurrence.OccurrenceId);
        }
    }
}