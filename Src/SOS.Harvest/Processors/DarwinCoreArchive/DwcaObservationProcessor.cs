using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation processor.
    /// </summary>
    public class DwcaObservationProcessor : ObservationProcessorBase<DwcaObservationProcessor, DwcObservationVerbatim, IDarwinCoreArchiveVerbatimRepository>,
        IDwcaObservationProcessor
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IAreaHelper _areaHelper;
        private readonly IVocabularyRepository _processedVocabularyRepository;
        private readonly DwcaConfiguration _dwcaConfiguration;

        protected override async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {

            if (_dwcaConfiguration.UseDwcaCollectionRepository)
            {
                return await ProcessObservationsUsingCollectionDwcAsync(dataProvider, taxa, mode, cancellationToken);
            }

            return await ProcessObservationsUsingDwcAsync(dataProvider, taxa, mode, cancellationToken);
        }

        private async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsUsingDwcAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            using var dwcArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(
                dataProvider,
                _verbatimClient,
                Logger);

            var observationFactory = await DwcaObservationFactory.CreateAsync(
                dataProvider,
                taxa,
                _processedVocabularyRepository,
                _areaHelper,
                TimeManager,
                ProcessConfiguration);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                dwcArchiveVerbatimRepository,
                cancellationToken);
        }

        private async Task<(int publicCount, int protectedCount, int failedCount)> ProcessObservationsUsingCollectionDwcAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            using var dwcCollectionRepository = new DwcCollectionRepository(dataProvider, _verbatimClient, Logger);

            var observationFactory = await DwcaObservationFactory.CreateAsync(
                dataProvider,
                taxa,
                _processedVocabularyRepository,
                _areaHelper,
                TimeManager,
                ProcessConfiguration);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                dwcCollectionRepository.OccurrenceRepository,                
                cancellationToken);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="areaHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="processManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DwcaObservationProcessor(
            IVerbatimClient verbatimClient,
            IProcessedObservationCoreRepository processedObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IAreaHelper areaHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            IDiffusionManager diffusionManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            DwcaConfiguration dwcaConfiguration,
            ILogger<DwcaObservationProcessor> logger) :
                base(processedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, processManager, validationManager, diffusionManager, processTimeManager, null, processConfiguration, logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
        }

        public override DataProviderType Type => DataProviderType.DwcA;
    }
}