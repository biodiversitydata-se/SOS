using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.DarwinCoreArchive
{
    /// <summary>
    /// DwC-A observation processor.
    /// </summary>
    public class DwcaObservationProcessor : ObservationProcessorBase<DwcaObservationProcessor>, Interfaces.IDwcaObservationProcessor
    {
        private readonly IDwcaVerbatimRepository _dwcaVerbatimRepository;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly ProcessConfiguration _processConfiguration;
        private readonly IAreaHelper _areaHelper;
        public override ObservationProvider DataProvider => ObservationProvider.Dwca;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwcaVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedFieldMappingRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        public DwcaObservationProcessor(
            IDwcaVerbatimRepository dwcaVerbatimRepository,
            IProcessedObservationRepository processedObservationRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            IAreaHelper areaHelper,
            ProcessConfiguration processConfiguration,
            ILogger<DwcaObservationProcessor> logger) : base(processedObservationRepository, fieldMappingResolverHelper, logger)
        {
            _dwcaVerbatimRepository = dwcaVerbatimRepository ?? throw new ArgumentNullException(nameof(dwcaVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processConfiguration = processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));

            if (processConfiguration == null)
            {
                throw new ArgumentNullException(nameof(processConfiguration));
            }
        }

        /// <summary>
        /// Process all observations
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<int> ProcessObservations(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            return await ProcessObservationsSequential(taxa, cancellationToken);
        }

        private async Task<int> ProcessObservationsSequential(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            const int dataProviderId = 6; // todo - change
            const string dataProviderIdentifier = "BirdRinging"; // todo - change
            var verbatimCount = 0;
            var observationFactory = await DwcaObservationFactory.CreateAsync(
                taxa, 
                _processedFieldMappingRepository,
                _areaHelper);
            ICollection<ProcessedObservation> sightings = new List<ProcessedObservation>();
            using var cursor = await _dwcaVerbatimRepository.GetAllByCursorAsync(dataProviderId, dataProviderIdentifier);

            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                var processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                sightings.Add(processedObservation);
                if (IsBatchFilledToLimit(sightings.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += await CommitBatchAsync(sightings);
                    Logger.LogDebug($"DwC-A sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (sightings.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += await CommitBatchAsync(sightings);
                Logger.LogDebug($"DwC-A sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}