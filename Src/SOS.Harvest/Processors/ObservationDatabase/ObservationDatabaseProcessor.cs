﻿using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.ObservationDatabase.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ObservationDatabase;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Processors.ObservationDatabase
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ObservationDatabaseProcessor : ObservationProcessorBase<ObservationDatabaseProcessor, ObservationDatabaseVerbatim, IObservationDatabaseVerbatimRepository>,
        IObservationDatabaseProcessor
    {
        private readonly IObservationDatabaseVerbatimRepository _observationDatabaseVerbatimRepository;
        private readonly IVocabularyRepository _processedVocabularyRepository;
        private readonly IAreaHelper _areaHelper;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new ObservationDatabaseObservationFactory(
                dataProvider, taxa, dwcaVocabularyById, _areaHelper, TimeManager, ProcessConfiguration);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                _observationDatabaseVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationDatabaseVerbatimRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ObservationDatabaseProcessor(IObservationDatabaseVerbatimRepository observationDatabaseVerbatimRepository,
            IProcessedObservationCoreRepository processedObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IDiffusionManager diffusionManager,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            ProcessConfiguration processConfiguration,
            ILogger<ObservationDatabaseProcessor> logger) :
                base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, processManager, validationManager, diffusionManager, processTimeManager, null, processConfiguration, logger)
        {
            _observationDatabaseVerbatimRepository = observationDatabaseVerbatimRepository ??
                                                     throw new ArgumentNullException(nameof(observationDatabaseVerbatimRepository));
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.ObservationDatabase;
    }
}