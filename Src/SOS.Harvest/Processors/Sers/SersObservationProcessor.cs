﻿using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Sers.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Processors.Sers
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class SersObservationProcessor : ObservationProcessorBase<SersObservationProcessor, SersObservationVerbatim, ISersObservationVerbatimRepository>,
        ISersObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly ISersObservationVerbatimRepository _sersObservationVerbatimRepository;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new SersObservationFactory(dataProvider, taxa, dwcaVocabularyById, _areaHelper, TimeManager, ProcessConfiguration);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                _sersObservationVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sersObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SersObservationProcessor(ISersObservationVerbatimRepository sersObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationCoreRepository processedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            IValidationManager validationManager,
            IDiffusionManager diffusionManager,
            ProcessConfiguration processConfiguration,
            ILogger<SersObservationProcessor> logger) :
            base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, processManager, validationManager, diffusionManager, processTimeManager, null, processConfiguration, logger)
        {
            _sersObservationVerbatimRepository = sersObservationVerbatimRepository ??
                                                 throw new ArgumentNullException(
                                                     nameof(sersObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.SersObservations;
    }
}