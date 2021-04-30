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
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.VirtualHerbarium.Interfaces;

namespace SOS.Process.Processors.VirtualHerbarium
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class VirtualHerbariumObservationProcessor : 
        ObservationProcessorBase<VirtualHerbariumObservationProcessor, VirtualHerbariumObservationVerbatim, VirtualHerbariumObservationFactory, IVirtualHerbariumObservationVerbatimRepository>,
        IVirtualHerbariumObservationProcessor
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IVirtualHerbariumObservationVerbatimRepository _virtualHerbariumObservationVerbatimRepository;

        /// <inheritdoc />
        protected override async Task<(int publicCount, int protectedCount)> ProcessObservations(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            var observationFactory = new VirtualHerbariumObservationFactory(dataProvider, taxa, _areaHelper);

            return await base.ProcessObservationsAsync(
                dataProvider,
                mode,
                observationFactory,
                _virtualHerbariumObservationVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="virtualHerbariumObservationVerbatimRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processedPublicObservationRepository"></param>
        /// <param name="vocabularyValueResolver"></param>
        /// <param name="dwcArchiveFileWriterCoordinator"></param>
        /// <param name="validationManager"></param>
        /// <param name="processConfiguration"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumObservationProcessor(
            IVirtualHerbariumObservationVerbatimRepository virtualHerbariumObservationVerbatimRepository,
            IAreaHelper areaHelper,
            IProcessedPublicObservationRepository processedPublicObservationRepository,
            IVocabularyValueResolver vocabularyValueResolver,
            IDwcArchiveFileWriterCoordinator dwcArchiveFileWriterCoordinator,
            IValidationManager validationManager,
            ProcessConfiguration processConfiguration,
            ILogger<VirtualHerbariumObservationProcessor> logger) : 
                base(processedPublicObservationRepository, vocabularyValueResolver, dwcArchiveFileWriterCoordinator, validationManager, processConfiguration, logger)
        {
            _virtualHerbariumObservationVerbatimRepository = virtualHerbariumObservationVerbatimRepository ??
                                                             throw new ArgumentNullException(
                                                                 nameof(virtualHerbariumObservationVerbatimRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public override DataProviderType Type => DataProviderType.VirtualHerbariumObservations;
    }
}