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
    public class ClamPortalProcessFactory : DataProviderProcessorBase<ClamPortalProcessFactory>, Interfaces.IClamPortalProcessFactory
    {
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;
        public override DataProvider DataProvider => DataProvider.ClamPortal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedSightingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="logger"></param>
        public ClamPortalProcessFactory(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedSightingRepository processedSightingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<ClamPortalProcessFactory> logger) : base(processedSightingRepository, fieldMappingResolverHelper, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <inheritdoc />
        public async Task<RunInfo> ProcessAsync(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;
            Logger.LogDebug("Start clam portal process job");
            try
            {
                Logger.LogDebug("Start deleting clam portal data");
                if (!await ProcessRepository.DeleteProviderDataAsync(DataProvider))
                {
                    Logger.LogError("Failed to delete clam portal data");
                    return RunInfo.Failed(DataProvider, startTime, DateTime.Now);
                }
                Logger.LogDebug("Finish deleting clam portal data");

                Logger.LogDebug("Start processing Clam Portal data");
                var verbatimCount = await ProcessObservations(taxa, cancellationToken);
                Logger.LogDebug($"Finish processing Clam Portal data.");
                
                return RunInfo.Success(DataProvider, startTime, DateTime.Now, verbatimCount);
            }
            catch (JobAbortedException)
            {
                Logger.LogInformation("Clam observation processing was canceled.");
                return RunInfo.Cancelled(DataProvider, startTime, DateTime.Now);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to process clams verbatim");
                return RunInfo.Failed(DataProvider, startTime, DateTime.Now);
            }
        }

        private async Task<int> ProcessObservations(IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedSighting> sightings = new List<ProcessedSighting>();
            
            using var cursor = await _clamObservationVerbatimRepository.GetAllAsync();

            // Process and commit in batches.
            await cursor.ForEachAsync(c =>
            {
                ProcessedSighting processedSighting = c.ToProcessed(taxa);
                _areaHelper.AddAreaDataToProcessedSighting(processedSighting);
                sightings.Add(processedSighting);
                if (IsBatchFilledToLimit(sightings.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += CommitBatch(sightings);
                    Logger.LogDebug($"Clam Portal Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (sightings.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += CommitBatch(sightings);
                Logger.LogDebug($"Clam Portal Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}
