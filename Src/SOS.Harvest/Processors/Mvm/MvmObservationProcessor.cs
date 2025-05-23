﻿using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Mvm.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Processors.Mvm
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class MvmObservationProcessor : ObservationProcessorBase<MvmObservationProcessor, MvmObservationVerbatim, IMvmObservationVerbatimRepository>, IMvmObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IMvmObservationVerbatimRepository _mvmObservationVerbatimRepository;
        private readonly IVocabularyRepository _processedVocabularyRepository;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new MvmObservationFactory(dataProvider, taxa, dwcaVocabularyById, _areaHelper, TimeManager, _processedVocabularyRepository, ProcessConfiguration);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                _mvmObservationVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mvmObservationVerbatimRepository"></param>
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
        public MvmObservationProcessor(IMvmObservationVerbatimRepository mvmObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationCoreRepository processedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            IDiffusionManager diffusionManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            IVocabularyRepository processedVocabularyRepository,
            ILogger<MvmObservationProcessor> logger) :
            base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, processManager, validationManager, diffusionManager, processTimeManager, null, processConfiguration, logger)
        {
            _mvmObservationVerbatimRepository = mvmObservationVerbatimRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(mvmObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _processedVocabularyRepository = processedVocabularyRepository ?? throw new ArgumentNullException(nameof(processedVocabularyRepository));
        }

        public override DataProviderType Type => DataProviderType.MvmObservations;
    }
}