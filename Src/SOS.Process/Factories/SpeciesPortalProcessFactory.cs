using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Shared;
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
        /// <param name="darwinCoreRepository"></param>
        /// <param name="logger"></param>
        public SpeciesPortalProcessFactory(
            ISpeciesPortalVerbatimRepository speciesPortalVerbatimRepository,
            IDarwinCoreRepository darwinCoreRepository,
            ILogger<SpeciesPortalProcessFactory> logger) : base(darwinCoreRepository, logger)
        {
            _speciesPortalVerbatimRepository = speciesPortalVerbatimRepository ?? throw new ArgumentNullException(nameof(speciesPortalVerbatimRepository));
        }

        /// <inheritdoc />
        public async Task<RunInfo> ProcessAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var runInfo = new RunInfo(DataProvider.KUL)
            {
                Start = DateTime.Now
            };

            try
            {
                Logger.LogDebug("Start Processing Species Portal Verbatim");

                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.Artdatabanken))
                {
                    Logger.LogError("Failed to delete Species Portal data");

                    runInfo.End = DateTime.Now;
                    runInfo.Status = RunStatus.Failed;
                    return runInfo;
                }

                Logger.LogDebug("Previous processed Species Portal data deleted");

                var verbatim = await _speciesPortalVerbatimRepository.GetBatchAsync(0);
                
                if (!verbatim.Any())
                {
                    Logger.LogError("No verbatim data to process");
                    runInfo.End = DateTime.Now;
                    runInfo.Status = RunStatus.Failed;
                    return runInfo;
                }

                var successCount = 0;
                var verbatimCount = 0;

                while (verbatim.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                   
                    var darwinCore = verbatim.ToDarwinCore(taxa).ToArray();

                    successCount += await ProcessRepository.AddManyAsync(darwinCore);

                    verbatimCount += verbatim.Count();

                    // Fetch next batch
                    verbatim = await _speciesPortalVerbatimRepository.GetBatchAsync(verbatimCount + 1);

                    Logger.LogInformation($"Species Portal observations being processed, totalCount: {verbatimCount}");
                }

                runInfo.Count = successCount;
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Success;
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Species Portal observation processing was canceled.");
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process sightings");
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Failed;
            }

            return runInfo;
        }
    }
}
