using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.DarwinCore;
using SOS.Process.Extensions;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class ClamTreePortalProcessFactory : ProcessBaseFactory<ClamTreePortalProcessFactory>, Interfaces.IClamTreePortalProcessFactory
    {
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly ITreeObservationVerbatimRepository _treeObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="treeObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedRepository"></param>
        /// <param name="logger"></param>
        public ClamTreePortalProcessFactory(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            ITreeObservationVerbatimRepository treeObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedRepository processedRepository,
            ILogger<ClamTreePortalProcessFactory> logger) : base(processedRepository, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _treeObservationVerbatimRepository = treeObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(treeObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public async Task<bool> ProcessAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogDebug("Start clam and tree portal process job");

            // Create task list
            var processTasks = new List<Task<bool>>
            {
                ProcessClamsAsync(taxa, cancellationToken),
                ProcessTreesAsync(taxa, cancellationToken)
            };

            // Run all tasks async
            await Task.WhenAll(processTasks);
            var success = processTasks.All(t => t.Result);

            Logger.LogDebug($"End clam and tree portal process job. Success: {success}");

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
                var count = verbatim.Count();

                if (count == 0)
                {
                    Logger.LogError("No clams verbatim data to process");
                    return false;
                }

                var totalCount = count;

                while (count > 0)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var dwcModels = verbatim.ToDarwinCore(taxa)?.ToArray() ?? new DarwinCore<DynamicProperties>[0];

                    // Add area related data to models
                    _areaHelper.AddAreaDataToDarwinCore(dwcModels);

                    await ProcessRepository.AddManyAsync(dwcModels);

                    verbatim = await _clamObservationVerbatimRepository.GetBatchAsync(totalCount + 1);
                    count = verbatim.Count();
                    totalCount += count;
                    Logger.LogInformation($"Clam observations being processed, totalCount={totalCount:N}");
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

        /// <summary>
        /// Process clams
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<bool> ProcessTreesAsync(
            IDictionary<int, DarwinCoreTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Start processing trees verbatim");

                var verbatim = await _treeObservationVerbatimRepository.GetBatchAsync(0);
                var count = verbatim.Count();

                if (count == 0)
                {
                    Logger.LogError("No tree verbatim data to process");
                    return false;
                }

                var totalCount = count;

                while (count > 0)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    var dwcModels = verbatim.ToDarwinCore(taxa)?.ToArray() ?? new DarwinCore<DynamicProperties>[0];

                    // Add area related data to models
                    _areaHelper.AddAreaDataToDarwinCore(dwcModels);

                    await ProcessRepository.AddManyAsync(dwcModels);

                    verbatim = await _treeObservationVerbatimRepository.GetBatchAsync(totalCount + 1);
                    count = verbatim.Count();
                    totalCount += count;
                    Logger.LogInformation($"Tree observations being processed, totalCount={totalCount:N}");
                }

                return true;
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Tree observation processing was canceled.");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process trees verbatim");
                return false;
            }
        }
    }
}
