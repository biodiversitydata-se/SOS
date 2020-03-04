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
    public class KulProcessFactory : ProcessBaseFactory<KulProcessFactory>, Interfaces.IKulProcessFactory
    {
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedSightingRepository"></param>
        /// <param name="logger"></param>
        public KulProcessFactory(
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedSightingRepository processedSightingRepository,
            ILogger<KulProcessFactory> logger) : base(processedSightingRepository, logger)
        {
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(kulObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public async Task<RunInfo> ProcessAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var runInfo = new RunInfo(DataProvider.KUL)
            {
                Start = DateTime.Now
            };

            try
            {
                Logger.LogDebug("Start Processing KUL Verbatim observations");

                Logger.LogDebug("Start deleting KUL Verbatim observations");
                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.KUL))
                {
                    Logger.LogError("Failed to delete KUL data");

                    runInfo.End = DateTime.Now;
                    runInfo.Status = RunStatus.Failed;
                    return runInfo;
                }
                Logger.LogDebug("Finsih deleting KUL Verbatim observations");

                Logger.LogDebug("Start getting KUL Verbatim observations");
                var verbatimCount = 0;
                var successCount = 0;
                using var cursor = await _kulObservationVerbatimRepository.GetAllAsync();

                ICollection<ProcessedSighting> sightings = new List<ProcessedSighting>();
                await cursor.ForEachAsync(c =>
                {
                    sightings.Add(c.ToProcessed(taxa));

                    if (sightings.Count % ProcessRepository.BatchSize == 0)
                    {
                        verbatimCount += ProcessRepository.BatchSize;
                        Logger.LogDebug($"KUL sightings processed: {verbatimCount}");
                        successCount += ProcessRepository.AddManyAsync(sightings).Result;
                        sightings.Clear();
                    }
                });

                if (sightings.Any())
                {
                    verbatimCount += sightings.Count;
                    Logger.LogDebug($"KUL Sightings processed: {verbatimCount}");
                    successCount += await ProcessRepository.AddManyAsync(sightings);
                    sightings.Clear();
                }
                Logger.LogDebug($"Finish Processing KUL Verbatim observations. {successCount} successful of {verbatimCount} verbatims");

                runInfo.End = DateTime.Now;
                runInfo.Count = successCount;
                runInfo.Status = RunStatus.Success;
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("KUL observation processing was canceled.");
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process KUL Verbatim observations");
                runInfo.End = DateTime.Now;
                runInfo.Status = RunStatus.Failed;
            }

            return runInfo;
        }
    }
}
