﻿using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.FishData.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.FishData;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Processors.FishData
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class FishDataObservationProcessor :
        ObservationProcessorBase<FishDataObservationProcessor, FishDataObservationVerbatim, IFishDataObservationVerbatimRepository>, IFishDataObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IFishDataObservationVerbatimRepository _fishDataObservationVerbatimRepository;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new FishDataObservationFactory(dataProvider, taxa, _areaHelper, TimeManager, ProcessConfiguration);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                _fishDataObservationVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fishDataObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="processManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FishDataObservationProcessor(
            IFishDataObservationVerbatimRepository fishDataObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationCoreRepository processedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            IDiffusionManager diffusionManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<FishDataObservationProcessor> logger) :
            base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, processManager, validationManager, diffusionManager, processTimeManager, null, processConfiguration, logger)
        {
            _fishDataObservationVerbatimRepository = fishDataObservationVerbatimRepository ??
                                                     throw new ArgumentNullException(
                                                         nameof(fishDataObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.FishDataObservations;
    }
}