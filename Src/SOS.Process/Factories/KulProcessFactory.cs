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
    public class KulProcessFactory : DataProviderProcessorBase<KulProcessFactory>, Interfaces.IKulProcessFactory
    {
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;
        public override DataProvider DataProvider => DataProvider.KUL;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedSightingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="logger"></param>
        public KulProcessFactory(
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedSightingRepository processedSightingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<KulProcessFactory> logger) : base(processedSightingRepository, fieldMappingResolverHelper,logger)
        {
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(kulObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public async Task<RunInfo> ProcessAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            Logger.LogDebug("Start Processing KUL Verbatim observations");
            var startTime = DateTime.Now;
            try
            {
                Logger.LogDebug("Start deleting KUL data");
                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider))
                {
                    Logger.LogError("Failed to delete KUL data");
                    return RunInfo.Failed(DataProvider, startTime, DateTime.Now);
                }
                Logger.LogDebug("Finish deleting KUL data");

                Logger.LogDebug("Start processing KUL data");
                var verbatimCount = await ProcessObservations(taxa, cancellationToken);
                Logger.LogDebug($"Finish processing KUL data.");

                return RunInfo.Success(DataProvider, startTime, DateTime.Now, verbatimCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("KUL observation processing was canceled.");
                return RunInfo.Cancelled(DataProvider, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process sightings");
                return RunInfo.Failed(DataProvider, startTime, DateTime.Now);
            }

        }

        private async Task<int> ProcessObservations(
                    IDictionary<int, ProcessedTaxon> taxa,
                    IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedSighting> sightings = new List<ProcessedSighting>();

            using var cursor = await _kulObservationVerbatimRepository.GetAllAsync();

            // Process and commit in batches.
            await cursor.ForEachAsync(async c =>
            {
                ProcessedSighting processedSighting = c.ToProcessed(taxa);
                _areaHelper.AddAreaDataToProcessedSighting(processedSighting);
                sightings.Add(processedSighting);
                if (IsBatchFilledToLimit(sightings.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += await CommitBatchAsync(sightings);
                    Logger.LogDebug($"KUL Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (sightings.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += await CommitBatchAsync(sightings);
                Logger.LogDebug($"KUL Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}
