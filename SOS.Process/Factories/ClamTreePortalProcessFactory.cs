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
    public class ClamTreePortalProcessFactory : ProcessBaseFactory<ClamTreePortalProcessFactory>, Interfaces.IClamTreePortalProcessFactory
    {
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly ITreeObservationVerbatimRepository _treeObservationVerbatimRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="treeObservationVerbatimRepository"></param>
        /// <param name="processedRepository"></param>
        /// <param name="logger"></param>
        public ClamTreePortalProcessFactory(IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            ITreeObservationVerbatimRepository treeObservationVerbatimRepository,
            IProcessedRepository processedRepository,
            ILogger<ClamTreePortalProcessFactory> logger) : base(processedRepository, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _treeObservationVerbatimRepository = treeObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(treeObservationVerbatimRepository));
        }

        /// <summary>
        /// Process verbatim data and store it in darwin core format
        /// </summary>
        /// <param name="taxa"></param>
        /// <returns></returns>
        public async Task<bool> ProcessAsync(IDictionary<string, DarwinCoreTaxon> taxa)
        {
            Logger.LogDebug("Start clam and tree portal process job");

            // Create task list
            var processTasks = new List<Task<bool>>
            {
                ProcessClamsAsync(taxa),
                ProcessTreesAsync(taxa)
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
        /// <returns></returns>
        private async Task<bool> ProcessClamsAsync(IDictionary<string, DarwinCoreTaxon> taxa)
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
                    await ProcessRepository.AddManyAsync(verbatim.ToDarwinCore(taxa));

                    verbatim = await _clamObservationVerbatimRepository.GetBatchAsync(totalCount + 1);
                    count = verbatim.Count();
                    totalCount += count;
                }

                return true;
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
        /// <returns></returns>
        private async Task<bool> ProcessTreesAsync(IDictionary<string, DarwinCoreTaxon> taxa)
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
                    await ProcessRepository.AddManyAsync(verbatim.ToDarwinCore(taxa));

                    verbatim = await _treeObservationVerbatimRepository.GetBatchAsync(totalCount + 1);
                    count = verbatim.Count();
                    totalCount += count;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process trees verbatim");
                return false;
            }
        }
    }
}
