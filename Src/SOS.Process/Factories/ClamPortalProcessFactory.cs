using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Process.Extensions;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class ClamPortalProcessFactory : ProcessBaseFactory<ClamPortalProcessFactory>, Interfaces.IClamPortalProcessFactory
    {
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedSightingRepository"></param>
        /// <param name="logger"></param>
        public ClamPortalProcessFactory(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedSightingRepository processedSightingRepository,
            ILogger<ClamPortalProcessFactory> logger) : base(processedSightingRepository, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public async Task<RunInfo> ProcessAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var runInfo = new RunInfo(DataProvider.ClamPortal)
            {
                Start = DateTime.Now
            };
            Logger.LogDebug("Start clam portal process job");

            Logger.LogDebug("Start deleting clam portal data");
            if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.ClamPortal))
            {
                Logger.LogError("Failed to delete clam portal data");

                runInfo.Status = RunStatus.Failed;
                runInfo.End = DateTime.Now;
                return runInfo;
            }
            Logger.LogDebug("Finish deleting clam portal data");

            await ProcessClamsAsync(taxa, runInfo, cancellationToken);

            Logger.LogDebug($"End clam portal process job. Result: {runInfo.Status.ToString()}");

            // return result of processing
            return runInfo;
        }

        /// <summary>
        /// Process clams
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="runInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="runinfo"></param>
        /// <returns></returns>
        private async Task ProcessClamsAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            RunInfo runInfo,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Start processing clams verbatim");

                var verbatimCount = 0;
                using var cursor = await _clamObservationVerbatimRepository.GetAllAsync();

                ICollection<ProcessedSighting> sightings = new List<ProcessedSighting>();
                await cursor.ForEachAsync(c =>
                {
                    ProcessedSighting processedSighting = c.ToProcessed(taxa);
                    _areaHelper.AddAreaDataToProcessedSighting(processedSighting);
                    sightings.Add(processedSighting);
                    if (sightings.Count % ProcessRepository.BatchSize == 0)
                    {
                        verbatimCount += ProcessRepository.BatchSize;
                        Logger.LogDebug($"Clam Portal sightings processed: {verbatimCount}");
                        ProcessRepository.AddManyAsync(sightings);
                        sightings.Clear();
                    }
                });

                if (sightings.Any())
                {
                    verbatimCount += sightings.Count;
                    Logger.LogDebug($"Clam Portal Sightings processed: {verbatimCount}");
                    await ProcessRepository.AddManyAsync(sightings);
                    sightings.Clear();
                }
                Logger.LogDebug($"Finish processing Clam Portal data. ");

                runInfo.End = DateTime.Now;
                runInfo.Count = verbatimCount;
                runInfo.Status = RunStatus.Success;
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Clam observation processing was canceled.");
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process clams verbatim");
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Failed;
            }
        }
    }
}
