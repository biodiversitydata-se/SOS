using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
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
        /// <param name="DarwinCoreRepository"></param>
        /// <param name="logger"></param>
        public SpeciesPortalProcessFactory(
            ISpeciesPortalVerbatimRepository speciesPortalVerbatimRepository,
            IDarwinCoreRepository DarwinCoreRepository,
            ILogger<SpeciesPortalProcessFactory> logger) : base(DarwinCoreRepository, logger)
        {
            _speciesPortalVerbatimRepository = speciesPortalVerbatimRepository ?? throw new ArgumentNullException(nameof(speciesPortalVerbatimRepository));
        }

        /// <inheritdoc />
        public async Task<bool> ProcessAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Start Processing Species Portal Verbatim");

                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.Artdatabanken))
                {
                    Logger.LogError("Failed to delete Species Portal data");

                    return false;
                }

                Logger.LogDebug("Previous processed Species Portal data deleted");

                var verbatim = await _speciesPortalVerbatimRepository.GetBatchAsync(0);
                
                if (!verbatim.Any())
                {
                    Logger.LogError("No verbatim data to process");
                    return false;
                }

                var totalCount = 0;

                while (verbatim.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                   
                    var darwinCore = verbatim.ToDarwinCore(taxa).ToArray();
                   
                    await ProcessRepository.AddManyAsync(darwinCore);

                    totalCount += verbatim.Count();

                    // Fetch next batch
                    verbatim = await _speciesPortalVerbatimRepository.GetBatchAsync(totalCount + 1);

                    Logger.LogInformation($"Species Portal observations being processed, totalCount: {totalCount}");
                }

                return true;
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Species Portal observation processing was canceled.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process sightings");
                return false;
            }
        }

      
    }
}
