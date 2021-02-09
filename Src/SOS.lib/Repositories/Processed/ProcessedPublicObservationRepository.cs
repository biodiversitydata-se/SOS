using System;
using System.Collections.Generic;
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
    public class ProcessedPublicObservationRepository : ProcessedObservationRepositoryBase,
        IProcessedPublicObservationRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="elasticClient"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedPublicObservationRepository(
            IProcessClient client,
            IElasticClient elasticClient,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<ProcessedPublicObservationRepository> logger
        ) : base(false, client, elasticClient, elasticConfiguration, logger)
        {
            
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Observation>> GetObservationsAsync(IEnumerable<string> occurrenceIds)
        {
            try
            {
                var searchResponse = await ElasticClient.SearchAsync<Observation>(s => s
                    .Index(IndexName)
                    .Source(s => s
                        .Includes(i => i.Fields(f => f.Occurrence, f => f.Location))
                    )
                   .Query(q => q
                        .Terms(t => t
                            .Field(f => f.Occurrence.OccurrenceId)
                            .Terms(occurrenceIds)
                        )
                    )
                );

                if (!searchResponse.IsValid)
                {
                    throw new InvalidOperationException(searchResponse.DebugInformation);
                }

                return searchResponse.Documents;
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return null;
            }
        }
    }
}