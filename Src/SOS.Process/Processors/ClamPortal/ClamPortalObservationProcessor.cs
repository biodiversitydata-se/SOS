using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.ClamPortal
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class ClamPortalObservationProcessor : ObservationProcessorBase<ClamPortalObservationProcessor>, Interfaces.IClamPortalObservationProcessor
    {
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;
        public override ObservationProvider DataProvider => ObservationProvider.ClamPortal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="logger"></param>
        public ClamPortalObservationProcessor(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<ClamPortalObservationProcessor> logger) : base(processedObservationRepository, fieldMappingResolverHelper, logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        protected override async Task<int> ProcessObservations(IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedObservation> observations = new List<ProcessedObservation>();
            var observationFactory = new ClamPortalObservationFactory(taxa);

            using var cursor = await _clamObservationVerbatimRepository.GetAllByCursorAsync();
            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                ProcessedObservation processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
                observations.Add(processedObservation);
                if (IsBatchFilledToLimit(observations.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += await CommitBatchAsync(observations);
                    observations.Clear();
                    Logger.LogDebug($"Clam Portal Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += await CommitBatchAsync(observations);
                Logger.LogDebug($"Clam Portal Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}