﻿using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Processors.Artportalen
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ArtportalenChecklistProcessor : ChecklistProcessorBase<ArtportalenChecklistProcessor, ArtportalenChecklistVerbatim, IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int>>,
        IArtportalenChecklistProcessor
    {
        private readonly IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> _artportalenVerbatimRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedChecklistRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ArtportalenChecklistProcessor(IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> artportalenVerbatimRepository,
            IProcessedChecklistRepository processedChecklistRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<ArtportalenChecklistProcessor> logger) :
                base(processedChecklistRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ??
                                             throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessChecklistsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            var checklistFactory = new ArtportalenChecklistFactory(dataProvider, TimeManager, ProcessConfiguration);

            return await base.ProcessChecklistsAsync(
                dataProvider,
                checklistFactory,
                _artportalenVerbatimRepository,
                cancellationToken);
        }

        public override DataProviderType Type => DataProviderType.ArtportalenObservations;
    }
}