﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
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
        /// <param name="ProcessedSightingRepository"></param>
        /// <param name="logger"></param>
        public KulProcessFactory(
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedSightingRepository ProcessedSightingRepository,
            ILogger<KulProcessFactory> logger) : base(ProcessedSightingRepository, logger)
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

                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.KUL))
                {
                    Logger.LogError("Failed to delete KUL data");

                    runInfo.End = DateTime.Now;
                    runInfo.Status = RunStatus.Failed;
                    return runInfo;
                }

                Logger.LogDebug("Previous processed KUL data deleted");

                var verbatim = await _kulObservationVerbatimRepository.GetBatchAsync(0);
                
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

                    var processed = verbatim.ToProcessed(taxa).ToArray();

                    // Add area related data to models
                    _areaHelper.AddAreaDataToProcessed(processed);

                    successCount += await ProcessRepository.AddManyAsync(processed);

                    verbatimCount += verbatim.Count();

                    // Fetch next batch
                    verbatim = await _kulObservationVerbatimRepository.GetBatchAsync(verbatimCount + 1);
                    
                    Logger.LogInformation($"KUL observations being processed, totalCount: {verbatimCount}");
                }

                Logger.LogDebug($"End KUL Verbatim observations process job. Success: true");
                
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
