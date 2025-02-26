using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.iNaturalist.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Processors.iNaturalist
{
    /// <summary>
    /// iNaturalist observation processor
    /// </summary>
    public class iNaturalistObservationProcessor :
        ObservationProcessorBase<iNaturalistObservationProcessor, iNaturalistVerbatimObservation, IiNaturalistObservationVerbatimRepository>,
        IiNaturalistObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IiNaturalistObservationVerbatimRepository _iNaturalistVerbatimRepository;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> dwcaVocabularyById,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {            
            var observationFactory = new iNaturalistObservationFactory(dataProvider, taxa, dwcaVocabularyById, _areaHelper, TimeManager, ProcessConfiguration, Logger);
            await observationFactory.InitializeAsync();

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                _iNaturalistVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iNaturalistVerbatimRepository"></param>
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
        public iNaturalistObservationProcessor(
            IiNaturalistObservationVerbatimRepository iNaturalistVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedObservationCoreRepository processedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            IDiffusionManager diffusionManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<iNaturalistObservationProcessor> logger) :
            base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, processManager, validationManager, diffusionManager, processTimeManager, null, processConfiguration, logger)
        {
            _iNaturalistVerbatimRepository = iNaturalistVerbatimRepository ?? throw new ArgumentNullException(nameof(iNaturalistVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.iNaturalistObservations;
    }
}