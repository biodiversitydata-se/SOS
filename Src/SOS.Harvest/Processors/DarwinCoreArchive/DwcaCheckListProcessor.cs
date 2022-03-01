using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Repositories.Verbatim;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation processor.
    /// </summary>
    public class DwcaCheckListProcessor : CheckListProcessorBase<DwcaCheckListProcessor, DwcEventOccurrenceVerbatim, IVerbatimRepositoryBase<DwcEventOccurrenceVerbatim, int>>,
        IDwcaCheckListProcessor
    {
        private readonly IVerbatimClient _verbatimClient;
        private readonly IAreaHelper _areaHelper;
        private readonly IVocabularyRepository _vocabularyRepository;

        /// <inheritdoc />
        protected override async Task<int> ProcessCheckListsAsync(
            DataProvider dataProvider,
            IJobCancellationToken cancellationToken)
        {
            using var dwcArchiveVerbatimRepository = new EventOccurrenceDarwinCoreArchiveVerbatimRepository(
                dataProvider,
                _verbatimClient,
                Logger);

            var checkListFactory = await DwcaCheckListFactory.CreateAsync(dataProvider, _vocabularyRepository, _areaHelper);

            return await base.ProcessCheckListsAsync(
                dataProvider,
                checkListFactory,
                dwcArchiveVerbatimRepository,
                cancellationToken);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbatimClient"></param>
        /// <param name="processedCheckListRepository"></param>
        /// <param name="processManager"></param>
        /// <param name="areaHelper"></param>
        /// <param name="vocabularyRepository"></param>
        /// <param name="logger"></param>
        public DwcaCheckListProcessor(
            IVerbatimClient verbatimClient,
            IProcessedCheckListRepository processedCheckListRepository,
            IProcessManager processManager,
            IAreaHelper areaHelper,
            IVocabularyRepository vocabularyRepository,
            ILogger<DwcaCheckListProcessor> logger) :
                base(processedCheckListRepository, processManager, logger)
        {
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _vocabularyRepository = vocabularyRepository ??
                                    throw new ArgumentNullException(nameof(vocabularyRepository));
        }

        public override DataProviderType Type => DataProviderType.DwcA;
    }
}