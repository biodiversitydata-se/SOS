using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Shark.Interfaces;

namespace SOS.Process.Processors.Shark
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class SharkObservationProcessor : ObservationProcessorBase<SharkObservationProcessor, SharkObservationVerbatim, SharkObservationFactory, ISharkObservationVerbatimRepository>,
        ISharkObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly ISharkObservationVerbatimRepository _sharkObservationVerbatimRepository;
        private readonly IGeometryManager _geometryManager;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new SharkObservationFactory(dataProvider, taxa, _areaHelper, _geometryManager);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                _sharkObservationVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sharkObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="processManager"></param>
        /// <param name="validationManager"></param>
        /// <param name="geometryManager"></param>
        /// <param name="logger"></param>
        public SharkObservationProcessor(ISharkObservationVerbatimRepository sharkObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IProcessManager processManager,
            IValidationManager validationManager,
            IGeometryManager geometryManager,
            ILogger<SharkObservationProcessor> logger) : 
            base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, processManager, logger)
        {
            _sharkObservationVerbatimRepository = sharkObservationVerbatimRepository ??
                                                  throw new ArgumentNullException(
                                                      nameof(sharkObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _geometryManager = geometryManager ?? throw new ArgumentNullException(nameof(geometryManager));
        }

        public override DataProviderType Type => DataProviderType.SharkObservations;
    }
}