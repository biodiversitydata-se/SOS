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
using SOS.Lib.Configuration.Process;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Search.Filters;
using SOS.Harvest.Processors.Artportalen;
using Microsoft.Extensions.Logging.Abstractions;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A event processor.
    /// </summary>
    public class DwcaEventProcessor : EventProcessorBase<DwcaEventProcessor, DwcEventOccurrenceVerbatim, IVerbatimRepositoryBase<DwcEventOccurrenceVerbatim, int>>,
        IDwcaEventProcessor
    {
        private readonly IVerbatimClient _verbatimClient;        
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly IVocabularyRepository _vocabularyRepository;
        public override DataProviderType Type => DataProviderType.DwcA;

        public DwcaEventProcessor(
            //IVerbatimRepositoryBase<ArtportalenChecklistVerbatim, int> artportalenVerbatimRepository,
            //IArtportalenVerbatimRepository artportalenVerbatimRepository // observation verbatim repository
            //    IVerbatimClient verbatimClient,
            IProcessedObservationCoreRepository processedObservationRepository,
            IObservationEventRepository observationEventRepository,
            IAreaHelper areaHelper,
            IVocabularyRepository vocabularyRepository,
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<DwcaEventProcessor> logger) :
                base(observationEventRepository, processManager, processTimeManager, processConfiguration, logger)
        {
            _processedObservationRepository = processedObservationRepository;
            _areaHelper = areaHelper;
            _vocabularyRepository = vocabularyRepository;
        }

        protected override async Task<int> ProcessEventsAsync(DataProvider dataProvider, IJobCancellationToken cancellationToken)
        {
            using var dwcCollectionRepository = new DwcCollectionRepository(dataProvider, _verbatimClient, new NullLogger<DwcCollectionRepository>());
            DwcaEventFactory dwcaEventFactory = await DwcaEventFactory.CreateAsync(dataProvider, _vocabularyRepository, _areaHelper, TimeManager, ProcessConfiguration);

            return await base.ProcessEventsAsync(
                dataProvider,
                dwcaEventFactory,
                dwcCollectionRepository.EventRepository,
                cancellationToken);

            // Artportalen implementation
            //int nrAddedEvents = await AddObservationEventsAsync(dataProvider);
            //return nrAddedEvents;

            //using var dwcArchiveVerbatimRepository = new EventOccurrenceDarwinCoreArchiveVerbatimRepository(
            //    dataProvider,
            //    _verbatimClient,
            //    Logger);

            //var checklistFactory = await DwcaChecklistFactory.CreateAsync(dataProvider, _vocabularyRepository, _areaHelper, TimeManager, ProcessConfiguration);

            //return await base.ProcessChecklistsAsync(
            //    dataProvider,
            //    checklistFactory,
            //    dwcArchiveVerbatimRepository,
            //    cancellationToken);
        }

        // Artportalen implementation
        //private async Task<int> AddObservationEventsAsync(DataProvider dataProvider)
        //{            
        //    Logger.LogInformation("Start AddObservationEventsAsync()");
        //    int batchSize = 5000;
        //    var filter = new SearchFilter(0);
        //    filter.IsPartOfDataStewardshipDataset = true;
        //    Logger.LogInformation($"AddObservationEventsAsync(). Read data from Observation index: {_processedObservationRepository.PublicIndexName}");
        //    var eventOccurrenceIds = await _processedObservationRepository.GetEventOccurrenceItemsAsync(filter);   
        //}        

    }
}