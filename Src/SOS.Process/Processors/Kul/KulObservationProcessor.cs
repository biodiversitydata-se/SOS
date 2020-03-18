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

namespace SOS.Process.Processors.Kul
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class KulObservationProcessor : ObservationProcessorBase<KulObservationProcessor>, Interfaces.IKulObservationProcessor
    {
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;
        public override DataProvider DataProvider => DataProvider.KUL;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="logger"></param>
        public KulObservationProcessor(
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<KulObservationProcessor> logger) : base(processedObservationRepository, fieldMappingResolverHelper,logger)
        {
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(kulObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }
       
        protected override async Task<int> ProcessObservations(
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedObservation> sightings = new List<ProcessedObservation>();
            var observationFactory = new KulObservationFactory(taxa);

            using var cursor = await _kulObservationVerbatimRepository.GetAllAsync();

            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                ProcessedObservation processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
                sightings.Add(processedObservation);
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
