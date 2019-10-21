using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.DarwinCore;
using SOS.Process.Extensions;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class SpeciesPortalProcessFactory : ProcessBaseFactory<SpeciesPortalProcessFactory>, Interfaces.ISpeciesPortalProcessFactory
    {
        private readonly ISpeciesPortalVerbatimRepository _speciesPortalVerbatimRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalVerbatimRepository"></param>
        /// <param name="processedRepository"></param>
        /// <param name="logger"></param>
        public SpeciesPortalProcessFactory(ISpeciesPortalVerbatimRepository speciesPortalVerbatimRepository,
            IProcessedRepository processedRepository,
            ILogger<SpeciesPortalProcessFactory> logger) : base(processedRepository, logger)
        {
            _speciesPortalVerbatimRepository = speciesPortalVerbatimRepository ?? throw new ArgumentNullException(nameof(speciesPortalVerbatimRepository));
        }

        /// <summary>
        /// Process verbatim data and store it in darwin core format
        /// </summary>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public async Task<bool> ProcessAsync(IDictionary<string, DarwinCoreTaxon> taxa)
        {
            try
            {
                Logger.LogDebug("Start Processing Species Portal Verbatim");

                var verbatim = await _speciesPortalVerbatimRepository.GetBatchAsync(0);
                var count = verbatim.Count();

                if (count == 0)
                {
                    Logger.LogError("No verbatim data to process");
                    return false;
                }

                var totalCount = count;

                while (count > 0)
                {
                    await ProcessRepository.AddManyAsync(verbatim.ToDarwinCore(taxa));

                    verbatim = await _speciesPortalVerbatimRepository.GetBatchAsync(totalCount + 1);
                    count = verbatim.Count();
                    totalCount += count;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process sightings");
                return false;
            }
        }

      
    }
}
