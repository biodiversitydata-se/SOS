using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Processors.Artportalen.Interfaces;

namespace SOS.Process.Processors.Artportalen
{
    /// <summary>
    ///     Process factory class
    /// </summary>
    public class ArtportalenCheckListProcessor : CheckListProcessorBase<ArtportalenCheckListProcessor, ArtportalenCheckListVerbatim, IVerbatimRepositoryBase<ArtportalenCheckListVerbatim, int>>,
        IArtportalenCheckListProcessor
    {
        private readonly IVerbatimRepositoryBase<ArtportalenCheckListVerbatim, int> _artportalenVerbatimRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenVerbatimRepository"></param>
        /// <param name="processedCheckListRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="logger"></param>
        public ArtportalenCheckListProcessor(IVerbatimRepositoryBase<ArtportalenCheckListVerbatim, int> artportalenVerbatimRepository,
            IProcessedCheckListRepository processedCheckListRepository,
            IProcessManager processManager,
            ILogger<ArtportalenCheckListProcessor> logger) :
                base(processedCheckListRepository, processManager, logger)
        {
            _artportalenVerbatimRepository = artportalenVerbatimRepository ??
                                             throw new ArgumentNullException(nameof(artportalenVerbatimRepository));
        }

        /// <inheritdoc />
        protected override async Task<int> ProcessCheckListsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            var checkListFactory = new ArtportalenCheckListFactory(dataProvider);

            return await base.ProcessCheckListsAsync(
                dataProvider,
                checkListFactory,
                _artportalenVerbatimRepository,
                cancellationToken);
        }

        public override DataProviderType Type => DataProviderType.ArtportalenObservations;
    }
}