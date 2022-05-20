using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;

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
        /// <param name="logger"></param>
        public ArtportalenChecklistProcessor(IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> artportalenVerbatimRepository,
            IProcessedChecklistRepository processedChecklistRepository,
            IProcessManager processManager,
            ILogger<ArtportalenChecklistProcessor> logger) :
                base(processedChecklistRepository, processManager, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ??
                                             throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessChecklistsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            var checklistFactory = new ArtportalenChecklistFactory(dataProvider);

            return await base.ProcessChecklistsAsync(
                dataProvider,
                checklistFactory,
                _artportalenVerbatimRepository,
                cancellationToken);
        }

        public override DataProviderType Type => DataProviderType.ArtportalenObservations;
    }
}