using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Processors.Sers
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class SersObservationProcessor : ObservationProcessorBase<SersObservationProcessor>, Interfaces.ISersObservationProcessor
    {
        private readonly ISersObservationVerbatimRepository _sersObservationVerbatimRepository;
        private readonly IAreaHelper _areaHelper;
        public override DataProviderType Type => DataProviderType.SersObservations;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sersObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="fieldMappingResolverHelper"></param>
        /// <param name="logger"></param>
        public SersObservationProcessor(
            ISersObservationVerbatimRepository sersObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationRepository processedObservationRepository,
            IFieldMappingResolverHelper fieldMappingResolverHelper,
            ILogger<SersObservationProcessor> logger) : base(processedObservationRepository, fieldMappingResolverHelper,logger)
        {
            _sersObservationVerbatimRepository = sersObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(sersObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }
       
        protected override async Task<int> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, ProcessedTaxon> taxa,
            IJobCancellationToken cancellationToken)
        {
            var verbatimCount = 0;
            ICollection<ProcessedObservation> observations = new List<ProcessedObservation>();
            var observationFactory = new SersObservationFactory(dataProvider, taxa);

            using var cursor = await _sersObservationVerbatimRepository.GetAllByCursorAsync();

            // Process and commit in batches.
            await cursor.ForEachAsync(async verbatimObservation =>
            {
                ProcessedObservation processedObservation = observationFactory.CreateProcessedObservation(verbatimObservation);
                _areaHelper.AddAreaDataToProcessedObservation(processedObservation);
                observations.Add(processedObservation);
                if (IsBatchFilledToLimit(observations.Count))
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    verbatimCount += await CommitBatchAsync(dataProvider, observations);
                    observations.Clear();
                    Logger.LogDebug($"SERS Sightings processed: {verbatimCount}");
                }
            });

            // Commit remaining batch (not filled to limit).
            if (observations.Any())
            {
                cancellationToken?.ThrowIfCancellationRequested();
                verbatimCount += await CommitBatchAsync(dataProvider, observations);
                Logger.LogDebug($"SERS Sightings processed: {verbatimCount}");
            }

            return verbatimCount;
        }
    }
}
