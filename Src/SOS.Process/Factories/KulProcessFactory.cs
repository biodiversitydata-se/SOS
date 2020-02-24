using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
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
                var verbatim = await _kulObservationVerbatimRepository.GetBatchAsync(string.Empty);
                
                if (!verbatim.Any())
                {
                    Logger.LogError("No verbatim data to process");

                    runInfo.End = DateTime.Now;
                    runInfo.Status = RunStatus.Failed;
                    return runInfo;
                }

                var successCount = 0;
                var verbatimCount = 0;
                var count = verbatim.Count();

                while (count != 0)
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var processed = verbatim.ToProcessed(taxa).ToArray();

                    // Add area related data to models
                    _areaHelper.AddAreaDataToProcessed(processed);

                    successCount += await ProcessRepository.AddManyAsync(processed);

                    verbatimCount += count;
                    Logger.LogInformation($"KUL observations being processed, totalCount: {verbatimCount}");

                    // Fetch next batch
                    verbatim = await _kulObservationVerbatimRepository.GetBatchAsync(verbatim.Last().Id);
                    count = verbatim.Count();
                }
                Logger.LogDebug("Finish getting KUL Verbatim observations. Success: true");

                runInfo.Count = successCount;
                runInfo.End = DateTime.Now;
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
