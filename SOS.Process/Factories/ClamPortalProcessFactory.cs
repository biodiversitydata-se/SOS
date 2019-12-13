using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Lib.Models.Shared.Shared;
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
        /// <param name="darwinCoreRepository"></param>
        /// <param name="logger"></param>
        public ClamPortalProcessFactory(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IDarwinCoreRepository darwinCoreRepository,
            ILogger<ClamPortalProcessFactory> logger) : base(darwinCoreRepository, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public async Task<RunInfo> ProcessAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var runInfo = new RunInfo(DataProvider.ClamPortal)
            {
                Start = DateTime.Now
            };
            Logger.LogDebug("Start clam portal process job");

            if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.ClamPortal))
            {
                Logger.LogError("Failed to delete clam portal data");

                runInfo.Status = RunStatus.Failed;
                runInfo.End = DateTime.Now;
                return runInfo;
            }

            Logger.LogDebug("Previous processed clam portal data deleted");

            await ProcessClamsAsync(taxa, runInfo, cancellationToken);

            Logger.LogDebug($"End clam portal process job. Result: {runInfo.Status.ToString()}");

            // return result of processing
            return runInfo;
        }

        /// <summary>
        /// Process clams
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="runinfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ProcessClamsAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            RunInfo runInfo,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Start processing clams verbatim");

                var verbatim = await _clamObservationVerbatimRepository.GetBatchAsync(0);

                if (!verbatim.Any())
                {
                    Logger.LogError("No clams verbatim data to process");
                    runInfo.Status = RunStatus.Failed;
                    return;
                }

                var successCount = 0;
                var verbatimCount = 0;
                while (verbatim.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var darwinCore = verbatim.ToDarwinCore(taxa).ToArray();

                    // Add area related data to models
                    _areaHelper.AddAreaDataToDarwinCore(darwinCore);

                    successCount += await ProcessRepository.AddManyAsync(darwinCore);
                    verbatimCount += verbatim.Count();

                    Logger.LogInformation($"Clam observations being processed, totalCount: {verbatimCount}");

                    // Fetch next batch
                    verbatim = await _clamObservationVerbatimRepository.GetBatchAsync(verbatimCount + 1);
                }

                runInfo.Count = successCount;
                runInfo.End = DateTime.Now;
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
