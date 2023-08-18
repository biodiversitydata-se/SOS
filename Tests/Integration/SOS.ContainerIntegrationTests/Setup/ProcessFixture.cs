using Microsoft.ApplicationInsights;
using SOS.ContainerIntegrationTests.Helpers;
using SOS.ContainerIntegrationTests.Stubs;
using SOS.ContainerIntegrationTests.TestData;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.ContainerIntegrationTests.Setup;
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
    private List<Taxon> _taxa;
    private IDatasetRepository _datasetRepository;
    private IEventRepository _eventRepository;
    private IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private ArtportalenObservationFactory _artportalenObservationFactory;
    private IVocabularyValueResolver _vocabularyValueResolver;
    private IArtportalenDatasetMetadataRepository _artportalenDatasetMetadataRepository;
    private DataProvider _testDataProvider = new DataProvider { Id = 1, Identifier = "TestDataProvider" };

    public ProcessFixture(IAreaHelper areaHelper,
        IProcessClient processClient,
        IVocabularyRepository vocabularyRepository,
        ITaxonRepository taxonRepository,
        IProcessTimeManager processTimeManager,
        ProcessConfiguration processConfiguration,
        IDatasetRepository observationDatasetRepository,
        IEventRepository observationEventRepository,
        IProcessedObservationCoreRepository processedObservationCoreRepository,
        IVocabularyValueResolver vocabularyValueResolver,
        IArtportalenDatasetMetadataRepository artportalenDatasetMetadataRepository)
    {
        _areaHelper = areaHelper;
        _processClient = processClient;
        _vocabularyRepository = vocabularyRepository;
        _taxonRepository = taxonRepository;
        _processTimeManager = processTimeManager;
        _processConfiguration = processConfiguration;

        _datasetRepository = observationDatasetRepository;
        _eventRepository = observationEventRepository;
        _processedObservationCoreRepository = processedObservationCoreRepository;
        _vocabularyValueResolver = vocabularyValueResolver;
        _artportalenDatasetMetadataRepository = artportalenDatasetMetadataRepository;

        InitializeAsync().Wait();
    }

    public static ServiceCollection GetServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddSingleton<IAreaHelper, AreaHelper>();
        serviceCollection.AddSingleton<IAreaRepository, AreaRepository>();
        serviceCollection.AddSingleton<ProcessFixture>();
        serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();
        serviceCollection.AddSingleton<ITaxonRepository, TaxonRepository>();
        serviceCollection.AddSingleton<IProcessTimeManager, ProcessTimeManager>();
        serviceCollection.AddSingleton<ProcessConfiguration>();
        serviceCollection.AddSingleton<TelemetryClient>();
        serviceCollection.AddSingleton<IElasticClientManager, ElasticClientTestManager>();
        serviceCollection.AddSingleton<IDatasetRepository, DatasetRepository>();
        serviceCollection.AddSingleton<IEventRepository, EventRepository>();
        serviceCollection.AddSingleton<IProcessedObservationRepository, ProcessedObservationRepository>();
        serviceCollection.AddSingleton<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
        var elasticConfiguration = CreateElasticSearchConfiguration();
        serviceCollection.AddSingleton(elasticConfiguration);
        serviceCollection.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
        serviceCollection.AddSingleton<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
        serviceCollection.AddSingleton<IVocabularyValueResolver, VocabularyValueResolver>();
        serviceCollection.AddSingleton<IArtportalenDatasetMetadataRepository, ArtportalenDatasetMetadataRepository>();
        serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();
        serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();
        VocabularyConfiguration vocabularyConfiguration = new VocabularyConfiguration()
        {
            ResolveValues = true,
            LocalizationCultureCode = "sv-SE"
        };
        serviceCollection.AddSingleton(vocabularyConfiguration);

        return serviceCollection;
    }

    private async Task InitializeAsync()
    {
        _taxa = await _taxonRepository.GetAllAsync();
        _taxaById = _taxa.ToDictionary(m => m.Id, m => m);
        
        _artportalenObservationFactory = await ArtportalenObservationFactory.CreateAsync(
            new DataProvider { Id = 1 },
            _taxaById,
            _vocabularyRepository,
            _artportalenDatasetMetadataRepository,
            false,
            "https://www.artportalen.se",
            _processTimeManager,
            _processConfiguration);
    }

    public async Task InitializeElasticsearchIndices()
    {
        await _datasetRepository.ClearCollectionAsync();
        await _eventRepository.ClearCollectionAsync();
        await _processedObservationCoreRepository.ClearCollectionAsync(false);
        await _processedObservationCoreRepository.ClearCollectionAsync(true);
    }

    public async Task ProcessAndAddObservationsToElasticSearch(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
    {
        var processedObservations = ProcessObservations(verbatimObservations);
        await AddObservationsToElasticsearchAsync(processedObservations);
    }

    public List<Observation> ProcessObservations(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
    {
        var processedObservations = new List<Observation>();
        bool diffuseIfSupported = false;
        foreach (var verbatimObservation in verbatimObservations)
        {
            var processedObservation = _artportalenObservationFactory.CreateProcessedObservation(verbatimObservation, diffuseIfSupported);
            processedObservations.Add(processedObservation);
        }

        _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, true);
        return processedObservations;
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
        foreach (var file in files)
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
        TestDatas.TestDataSet testDataSet,
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
            await _datasetRepository.DeleteAllDocumentsAsync();
        }
        await _datasetRepository.DisableIndexingAsync();
        await _datasetRepository.AddManyAsync(datasets);
        await _datasetRepository.EnableIndexingAsync();
        await Task.Delay(delayInMs);
    }

    public async Task AddEventsToElasticsearchAsync(IEnumerable<Lib.Models.Processed.DataStewardship.Event.Event> events, bool clearExistingObservations = true, int delayInMs = 1000)
    {
        if (clearExistingObservations)
        {
            await _eventRepository.DeleteAllDocumentsAsync();
        }
        await _eventRepository.DisableIndexingAsync();
        await _eventRepository.AddManyAsync(events);
        await _eventRepository.EnableIndexingAsync();
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

    public async Task<long> GetObservationsCount(bool protectedIndex = false)
    {
        return await _processedObservationCoreRepository.IndexCountAsync(protectedIndex);
    }

    public async Task<long> GetEventsCount()
    {
        return await _eventRepository.IndexCountAsync();
    }

    public async Task<long> GetDatasetsCount()
    {
        return await _datasetRepository.IndexCountAsync();
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

    private static ElasticSearchConfiguration CreateElasticSearchConfiguration()
    {
        return new ElasticSearchConfiguration()
        {
            IndexSettings = new List<ElasticSearchIndexConfiguration>()
                {
                    new ElasticSearchIndexConfiguration
                    {
                        Name = "observation",
                        ReadBatchSize = 10000,
                        WriteBatchSize = 1000,
                        ScrollBatchSize = 5000,
                        ScrollTimeout = "300s",
                    },
                    new ElasticSearchIndexConfiguration
                    {
                        Name = "observationEvent",
                        ReadBatchSize = 10000,
                        WriteBatchSize = 1000,
                        ScrollBatchSize = 5000,
                        ScrollTimeout = "300s"
                    },
                    new ElasticSearchIndexConfiguration
                    {
                        Name = "observationDataset",
                        ReadBatchSize = 10000,
                        WriteBatchSize = 1000,
                        ScrollBatchSize = 5000,
                        ScrollTimeout = "300s"
                    }
                },
            RequestTimeout = 300,
            DebugMode = true,
            IndexPrefix = "",
            Clusters = null
        };
    }
}
