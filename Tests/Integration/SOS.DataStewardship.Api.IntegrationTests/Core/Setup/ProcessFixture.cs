using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.DataStewardship.Api.IntegrationTests.Core.Setup
{
    public class ProcessFixture
    {
        private IProcessClient _processClient;
        private IAreaHelper _areaHelper;
        private DwcaObservationFactory _darwinCoreFactory;
        private DwcaEventFactory _dwcaEventFactory;
        private DwcaDatasetFactory _dwcaDatasetFactory;
        private IVocabularyRepository _vocabularyRepository;
        private Dictionary<int, Taxon> _taxaById;
        private ITaxonRepository _taxonRepository;
        private IProcessTimeManager _processTimeManager;
        private ProcessConfiguration _processConfiguration;

        private DataProvider _testDataProvider = new DataProvider { Id = 1, Identifier = "TestDataProvider" };
        private List<Taxon> _taxa;

        private IDatasetRepository _observationDatasetRepository;
        private IEventRepository _observationEventRepository;
        private IProcessedObservationCoreRepository _processedObservationCoreRepository;


        public ProcessFixture(IAreaHelper areaHelper,
            IProcessClient processClient,
            IVocabularyRepository vocabularyRepository,
            ITaxonRepository taxonRepository,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,

            IDatasetRepository observationDatasetRepository,
            IEventRepository observationEventRepository,
            IProcessedObservationCoreRepository processedObservationCoreRepository
            )
        {
            _areaHelper = areaHelper;
            _processClient = processClient;
            _vocabularyRepository = vocabularyRepository;
            _taxonRepository = taxonRepository;
            _processTimeManager = processTimeManager;
            _processConfiguration = processConfiguration;

            _observationDatasetRepository = observationDatasetRepository;
            _observationEventRepository = observationEventRepository;
            _processedObservationCoreRepository = processedObservationCoreRepository;

            InitializeAsync().Wait();
        }

        private async Task InitializeAsync()
        {
            _taxa = await _taxonRepository.GetAllAsync();
            _taxaById = _taxa.ToDictionary(m => m.Id, m => m);
        }

        public async Task InitializeElasticsearchIndices()
        {
            await _observationDatasetRepository.ClearCollectionAsync();
            await _observationEventRepository.ClearCollectionAsync();
            await _processedObservationCoreRepository.ClearCollectionAsync(false);
            await _processedObservationCoreRepository.ClearCollectionAsync(true);
        }

        public async Task ImportDwcaFileAsync(string filePath, DataProvider dataProvider, ITestOutputHelper output)
        {            
            var parsedDwcaFile = await DwcaHelper.ReadDwcaFileAsync(filePath, dataProvider);
            var observationFactory = GetDwcaObservationFactory(true);
            var eventFactory = GetDwcaEventFactory(true);
            var datasetFactory = GetDwcaDatasetFactory();

            var processedObservations = parsedDwcaFile
                .Occurrences
                .Select(m => observationFactory.CreateProcessedObservation(m, false))
                .ToList();
            await AddObservationsToElasticsearchAsync(processedObservations);
            output.WriteLine($"Processed observations count= {processedObservations.Count}");

            var processedEvents = parsedDwcaFile
                .Events
                .Select(m => eventFactory.CreateEventObservation(m))
                .ToList();
            await AddEventsToElasticsearchAsync(processedEvents);
            output.WriteLine($"Processed events count= {processedEvents.Count}");

            var processedDatasets = parsedDwcaFile
                .Datasets
                .Select(m => datasetFactory.CreateProcessedDataset(m))
                .ToList();
            await AddDatasetsToElasticsearchAsync(processedDatasets);
            output.WriteLine($"Processed datasets count= {processedDatasets.Count}");
        }

        public async Task ImportDwcaFilesAsync(IEnumerable<(string filePath, DataProvider dataProvider)> files, ITestOutputHelper output)
        {
            bool clearExistingRecords = true;
            foreach(var file in files)
            {
                var parsedDwcaFile = await DwcaHelper.ReadDwcaFileAsync(file.filePath, file.dataProvider);
                var observationFactory = GetDwcaObservationFactory(true);
                var eventFactory = GetDwcaEventFactory(true);
                var datasetFactory = GetDwcaDatasetFactory();

                var processedObservations = parsedDwcaFile
                    .Occurrences
                    .Select(m => observationFactory.CreateProcessedObservation(m, false))
                    .ToList();
                await AddObservationsToElasticsearchAsync(processedObservations, false, clearExistingRecords);
                output.WriteLine($"Processed observations count= {processedObservations.Count}");

                var processedEvents = parsedDwcaFile
                    .Events
                    .Select(m => eventFactory.CreateEventObservation(m))
                    .ToList();
                await AddEventsToElasticsearchAsync(processedEvents, clearExistingRecords);
                output.WriteLine($"Processed events count= {processedEvents.Count}");

                var processedDatasets = parsedDwcaFile
                    .Datasets
                    .Select(m => datasetFactory.CreateProcessedDataset(m))
                    .ToList();
                await AddDatasetsToElasticsearchAsync(processedDatasets, clearExistingRecords);
                output.WriteLine($"Processed datasets count= {processedDatasets.Count}");
                clearExistingRecords = false;
            }
        }


        public async Task AddDataToElasticsearchAsync(
            List<Dataset> datasets, 
            List<Lib.Models.Processed.DataStewardship.Event.Event> events, 
            List<Observation> observations,
            bool protectedIndex = false,
            bool clearExistingObservations = true)
        {
            await AddDatasetsToElasticsearchAsync(datasets, clearExistingObservations, 0);
            await AddEventsToElasticsearchAsync(events, clearExistingObservations, 0);
            await AddObservationsToElasticsearchAsync(observations, protectedIndex, clearExistingObservations, 0);
            await Task.Delay(1000);
        }

        public async Task AddDataToElasticsearchAsync(
            TestData.TestDataSet testDataSet,
            bool protectedIndex = false,
            bool clearExistingObservations = true)
        {
            await AddDatasetsToElasticsearchAsync(testDataSet.Datasets, clearExistingObservations, 0);
            await AddEventsToElasticsearchAsync(testDataSet.Events, clearExistingObservations, 0);
            await AddObservationsToElasticsearchAsync(testDataSet.Observations, protectedIndex, clearExistingObservations, 0);
            await Task.Delay(1000);
        }        

        public async Task AddDataToElasticsearchAsync(
            IEnumerable<Lib.Models.Processed.DataStewardship.Event.Event> events, 
            IEnumerable<Observation> observations,
            bool protectedIndex = false,
            bool clearExistingObservations = true)
        {
            await AddEventsToElasticsearchAsync(events, clearExistingObservations, 0);
            await AddObservationsToElasticsearchAsync(observations, protectedIndex, clearExistingObservations, 0);
            await Task.Delay(1000);
        }


        public async Task AddDatasetsToElasticsearchAsync(IEnumerable<Dataset> datasets, bool clearExistingObservations = true, int delayInMs = 1000)
        {
            if (clearExistingObservations)
            {
                await _observationDatasetRepository.DeleteAllDocumentsAsync();
            }
            await _observationDatasetRepository.DisableIndexingAsync();
            await _observationDatasetRepository.AddManyAsync(datasets);
            await _observationDatasetRepository.EnableIndexingAsync();
            await Task.Delay(delayInMs);
        }

        public async Task AddEventsToElasticsearchAsync(IEnumerable<Lib.Models.Processed.DataStewardship.Event.Event> events, bool clearExistingObservations = true, int delayInMs = 1000)
        {
            if (clearExistingObservations)
            {
                await _observationEventRepository.DeleteAllDocumentsAsync();
            }
            await _observationEventRepository.DisableIndexingAsync();
            await _observationEventRepository.AddManyAsync(events);
            await _observationEventRepository.EnableIndexingAsync();
            await Task.Delay(delayInMs);
        }

        public async Task AddObservationsToElasticsearchAsync(IEnumerable<Observation> observations, bool protectedIndex = false, bool clearExistingObservations = true, int delayInMs = 1000)
        {
            if (clearExistingObservations)
            {
                await _processedObservationCoreRepository.DeleteAllDocumentsAsync(protectedIndex);
            }
            await _processedObservationCoreRepository.DisableIndexingAsync(protectedIndex);
            await _processedObservationCoreRepository.AddManyAsync(observations, protectedIndex);
            await _processedObservationCoreRepository.EnableIndexingAsync(protectedIndex);
            await Task.Delay(delayInMs);
        }

        public DwcaObservationFactory GetDwcaObservationFactory(bool initAreaHelper)
        {
            if (initAreaHelper) InitAreaHelper();
            if (_darwinCoreFactory == null)
            {
                _darwinCoreFactory = CreateDwcaObservationFactoryAsync(_testDataProvider).Result;
            }

            return _darwinCoreFactory;
        }

        public DwcaEventFactory GetDwcaEventFactory(bool initAreaHelper)
        {
            if (initAreaHelper) InitAreaHelper();
            if (_dwcaEventFactory == null)
            {
                _dwcaEventFactory = CreateDwcaEventFactoryAsync(_testDataProvider).Result;
            }

            return _dwcaEventFactory;
        }

        public DwcaDatasetFactory GetDwcaDatasetFactory()
        {
            if (_dwcaDatasetFactory == null)
            {
                _dwcaDatasetFactory = new DwcaDatasetFactory(_testDataProvider, _processTimeManager, _processConfiguration);
            }

            return _dwcaDatasetFactory;
        }

        private void InitAreaHelper()
        {
            if (!_areaHelper.IsInitialized)
            {
                _areaHelper.InitializeAsync().Wait();
            }
        }

        private async Task<DwcaEventFactory> CreateDwcaEventFactoryAsync(DataProvider dataProvider)
        {
            var factory = await DwcaEventFactory.CreateAsync(
                dataProvider,
                _vocabularyRepository,
                _areaHelper,
                _processTimeManager,
                _processConfiguration);

            return factory;
        }

        private async Task<DwcaObservationFactory> CreateDwcaObservationFactoryAsync(DataProvider dataProvider)
        {
            var factory = await DwcaObservationFactory.CreateAsync(
                dataProvider,
                _taxaById,
                _vocabularyRepository,
                _areaHelper,
                _processTimeManager,
                _processConfiguration);

            return factory;
        }
    }
}