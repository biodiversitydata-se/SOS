using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation processor.
    /// </summary>
    public class DwcaObservationProcessor : ObservationProcessorBase<DwcaObservationProcessor, DwcObservationVerbatim, DwcaObservationFactory, IDarwinCoreArchiveVerbatimRepository>,
        IDwcaObservationProcessor
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IAreaHelper _areaHelper;
        private readonly IVocabularyRepository _processedVocabularyRepository;

        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
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
                _areaHelper);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                dwcArchiveVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="processedVocabularyRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="areaHelper"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="processManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="logger"></param>
        public DwcaObservationProcessor(
            IVerbatimClient verbatimClient,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyRepository processedVocabularyRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IAreaHelper areaHelper,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            ILogger<DwcaObservationProcessor> logger) :
                base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, processManager, logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _processedVocabularyRepository = processedVocabularyRepository ??
                                               throw new ArgumentNullException(nameof(processedVocabularyRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.DwcA;
    }
}