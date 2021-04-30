using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ObservationDatabase;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.ObservationDatabase.Interfaces;

namespace SOS.Process.Processors.ObservationDatabase
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ObservationDatabaseProcessor : ObservationProcessorBase<ObservationDatabaseProcessor, ObservationDatabaseVerbatim, ObservationDatabaseObservationFactory, IObservationDatabaseVerbatimRepository>, 
        IObservationDatabaseProcessor
    {
        private readonly IObservationDatabaseVerbatimRepository _observationDatabaseVerbatimRepository;
        private readonly IAreaHelper _areaHelper;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new ObservationDatabaseObservationFactory(dataProvider, taxa, _areaHelper);

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
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="processedProtectedObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="diffusionManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="areaHelper"></param>
        /// <param name="logger"></param>
        public ObservationDatabaseProcessor(IObservationDatabaseVerbatimRepository observationDatabaseVerbatimRepository,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IProcessedProtectedObservationRepository processedProtectedObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IDiffusionManager diffusionManager,
            IValidationManager validationManager,
            IAreaHelper areaHelper,
            ProcessConfiguration processConfiguration,
            ILogger<ObservationDatabaseProcessor> logger) : 
                base(processedPublicObservationRepository, processedProtectedObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, diffusionManager, processConfiguration, logger)
        {
            _observationDatabaseVerbatimRepository = observationDatabaseVerbatimRepository ??
                                                     throw new ArgumentNullException(nameof(observationDatabaseVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.ObservationDatabase;
    }
}