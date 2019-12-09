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
        /// <param name="DarwinCoreRepository"></param>
        /// <param name="logger"></param>
        public ClamPortalProcessFactory(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IDarwinCoreRepository DarwinCoreRepository,
            ILogger<ClamPortalProcessFactory> logger) : base(DarwinCoreRepository, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public async Task<bool> ProcessAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogDebug("Start clam portal process job");

            if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider.ClamPortal))
            {
                Logger.LogError("Failed to delete clam portal data");

                return false;
            }

            Logger.LogDebug("Previous processed clam portal data deleted");

            var success = await ProcessClamsAsync(taxa, cancellationToken);

            Logger.LogDebug($"End clam portal process job. Success: {success}");

            // return result of all harvests
            return success;
        }

        /// <summary>
        /// Process clams
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> ProcessClamsAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Start processing clams verbatim");

                var verbatim = await _clamObservationVerbatimRepository.GetBatchAsync(0);

                if (!verbatim.Any())
                {
                    Logger.LogError("No clams verbatim data to process");
                    return false;
                }

                var totalCount = 0;

                while (verbatim.Any())
                {
                    cancellationToken?.ThrowIfCancellationRequested();

                    var darwinCore = verbatim.ToDarwinCore(taxa).ToArray();

                    // Add area related data to models
                    _areaHelper.AddAreaDataToDarwinCore(darwinCore);

                    await ProcessRepository.AddManyAsync(darwinCore);

                    totalCount += verbatim.Count();

                    // Fetch next batch
                    verbatim = await _clamObservationVerbatimRepository.GetBatchAsync(totalCount + 1);

                    Logger.LogInformation($"Clam observations being processed, totalCount: {totalCount}");
                }

                return true;
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Clam observation processing was canceled.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process clams verbatim");
                return false;
            }
        }
    }
}
