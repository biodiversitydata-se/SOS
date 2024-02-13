using DwC_A;
using Microsoft.ApplicationInsights;
using SOS.Harvest.DarwinCore;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Harvest.Managers;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen;
using SOS.Harvest.Processors.DarwinCoreArchive;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Checklist;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.IntegrationTests.Setup.ContainerDbFixtures;
public class ProcessFixture : IProcessFixture
{
    private IProcessClient _processClient;
    private IAreaHelper _areaHelper;
    private DwcaObservationFactory? _darwinCoreFactory;
    private DwcaEventFactory? _dwcaEventFactory;
    private DwcaDatasetFactory? _dwcaDatasetFactory;
    private IVocabularyRepository _vocabularyRepository;
    private Dictionary<int, Taxon>? _taxaById;
    private ITaxonRepository _taxonRepository;
    private IProcessTimeManager _processTimeManager;
    private ProcessConfiguration _processConfiguration;
    private List<Taxon>? _taxa;
    private IDatasetRepository _datasetRepository;
    private IEventRepository _eventRepository;
    private IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private ArtportalenObservationFactory? _artportalenObservationFactory;
    private IVocabularyValueResolver _vocabularyValueResolver;
    private IArtportalenDatasetMetadataRepository _artportalenDatasetMetadataRepository;
    private ArtportalenChecklistFactory? _artportalenChecklistFactory;
    private IProcessedChecklistRepository _processedChecklistRepository { get; set; }
    private DataProvider _testDataProvider = new DataProvider { Id = 1, Identifier = "TestDataProvider" };

    public List<Taxon> Taxa
    {
        get => _taxa!;
    }

    public Dictionary<int, Taxon> TaxonById
    {
        get => _taxaById!;
    }

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
        IArtportalenDatasetMetadataRepository artportalenDatasetMetadataRepository,
        IProcessedChecklistRepository processedChecklistRepository)
    {
        _processClient = processClient;
        _areaHelper = areaHelper;
        _vocabularyRepository = vocabularyRepository;
        _taxonRepository = taxonRepository;
        _processTimeManager = processTimeManager;
        _processConfiguration = processConfiguration;

        _datasetRepository = observationDatasetRepository;
        _eventRepository = observationEventRepository;
        _processedObservationCoreRepository = processedObservationCoreRepository;
        _vocabularyValueResolver = vocabularyValueResolver;
        _artportalenDatasetMetadataRepository = artportalenDatasetMetadataRepository;
        _processedChecklistRepository = processedChecklistRepository;

        InitializeAsync().Wait();
    }

    public static ServiceCollection GetServiceCollection()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMemoryCache();
        serviceCollection.AddSingleton<IAreaHelper, AreaHelper>();
        serviceCollection.AddSingleton<IAreaRepository, AreaRepository>();
        serviceCollection.AddSingleton<IProcessFixture, ProcessFixture>();
        serviceCollection.AddSingleton<IVocabularyRepository, VocabularyRepository>();
        serviceCollection.AddSingleton<ITaxonRepository, TaxonRepository>();
        serviceCollection.AddSingleton<IProcessTimeManager, ProcessTimeManager>();
        serviceCollection.AddSingleton(GetProcessConfiguration());
        serviceCollection.AddSingleton<TelemetryClient>();
        serviceCollection.AddSingleton<IElasticClientManager, ElasticClientTestManager>();
        serviceCollection.AddSingleton<IDatasetRepository, DatasetRepository>();
        serviceCollection.AddSingleton<IEventRepository, EventRepository>();
        serviceCollection.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
        serviceCollection.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
        serviceCollection.AddSingleton<ITaxonListRepository, TaxonListRepository>();
        serviceCollection.AddSingleton<ITaxonManager, TaxonManager>();
        serviceCollection.AddSingleton<IProcessedTaxonRepository, ProcessedTaxonRepository>();
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
        serviceCollection.AddSingleton<IProcessedChecklistRepository, ProcessedChecklistRepository>();
        VocabularyConfiguration vocabularyConfiguration = new VocabularyConfiguration()
        {
            ResolveValues = true,
            LocalizationCultureCode = "sv-SE"
        };
        serviceCollection.AddSingleton(vocabularyConfiguration);

        return serviceCollection;
    }

    private static ProcessConfiguration GetProcessConfiguration()
    {
        return new ProcessConfiguration
        {
            NoOfThreads = 4,
            RunIncrementalAfterFull = true,
            VocabularyConfiguration = new VocabularyConfiguration
            {
                ResolveValues = false,
                LocalizationCultureCode = "sv-SE"
            },
            TaxonAttributeServiceConfiguration = new TaxonAttributeServiceConfiguration
            {
                BaseAddress = "https://taxonattributeservice.artdata.slu.se/api",
                AcceptHeaderContentType = "application/json"
            },
            TaxonServiceConfiguration = new TaxonServiceConfiguration
            {
                AcceptHeaderContentType = "application/text",
                BaseAddress = "https://taxonapi.artdata.slu.se/darwincore/download?version=custom"
            },
            Export_Container = "sos-export",
            MinObservationCount = 0,
            MinObservationProtectedCount = 0,
            ArtportalenUrl = "https://www.artportalen.se",
            ProcessUserObservation = false,
            ProcessDataset = true
        };
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

        _artportalenChecklistFactory = new ArtportalenChecklistFactory(new DataProvider { Id = 1 }, _processTimeManager, _processConfiguration);
    }

    public async Task InitializeElasticsearchIndices()
    {
        await _datasetRepository.ClearCollectionAsync();
        await _eventRepository.ClearCollectionAsync();
        await _processedObservationCoreRepository.ClearCollectionAsync(false);
        await _processedObservationCoreRepository.ClearCollectionAsync(true);
        await _processedChecklistRepository.ClearCollectionAsync();
    }

    public async Task CleanElasticsearchIndices()
    {
        await _datasetRepository.DeleteAllDocumentsAsync(waitForCompletion: true);
        await _eventRepository.DeleteAllDocumentsAsync(waitForCompletion: true);
        await _processedObservationCoreRepository.DeleteAllDocumentsAsync(protectedIndex: false, waitForCompletion: true);
        await _processedObservationCoreRepository.DeleteAllDocumentsAsync(protectedIndex: true, waitForCompletion: true);
        await _processedChecklistRepository.DeleteAllDocumentsAsync(waitForCompletion: true);
    }

    public async Task<List<Observation>> ProcessAndAddObservationsToElasticSearch(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
    {
        var processedObservations = ProcessObservations(verbatimObservations);
        await AddObservationsToElasticsearchAsync(processedObservations, true, 0);
        return processedObservations;
    }

    public List<Observation> ProcessObservations(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
    {
        var processedObservations = new List<Observation>();
        bool diffuseIfSupported = false;
        foreach (var verbatimObservation in verbatimObservations)
        {
            var processedObservation = _artportalenObservationFactory!.CreateProcessedObservation(verbatimObservation, diffuseIfSupported);
            processedObservations.Add(processedObservation);
        }

        _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, true);
        return processedObservations;
    }

    public List<Observation> ProcessObservations(IEnumerable<DwcObservationVerbatim> verbatimObservations, bool initAreaHelper = false)
    {
        var processedObservations = new List<Observation>();
        bool diffuseIfSupported = false;
        var factory = GetDarwinCoreFactory(initAreaHelper);
        foreach (var verbatimObservation in verbatimObservations)
        {
            var processedObservation = factory.CreateProcessedObservation(verbatimObservation, diffuseIfSupported);
            processedObservations.Add(processedObservation!);
        }

        return processedObservations;
    }

    public DwcaObservationFactory GetDarwinCoreFactory(bool initAreaHelper)
    {
        if (_darwinCoreFactory == null)
        {
            var dataProvider = new DataProvider() { Id = 1, Identifier = "Artportalen" };
            _areaHelper = new AreaHelper(new AreaRepository(_processClient, new NullLogger<AreaRepository>()));
            _darwinCoreFactory = CreateDarwinCoreFactoryAsync(dataProvider).Result;
        }

        if (initAreaHelper && !_areaHelper.IsInitialized)
        {
            _areaHelper.InitializeAsync().Wait();
        }

        return _darwinCoreFactory;
    }

    public async Task<DwcaObservationFactory> CreateDarwinCoreFactoryAsync(DataProvider dataProvider)
    {
        var factory = await DwcaObservationFactory.CreateAsync(
            dataProvider,
            _taxaById!,
            _vocabularyRepository,
            _areaHelper,
            _processTimeManager,
            _processConfiguration);

        return factory;
    }

    public async Task ImportDwcaFileUsingDwcArchiveReaderAsync(string filePath, DataProvider dataProvider, ITestOutputHelper output)
    {
        filePath = filePath.GetAbsoluteFilePath();
        string outputPath = Path.GetTempPath();
        using var archiveReader = new ArchiveReader(filePath, outputPath);
        IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(dataProvider, 0);
        var dwcObservations = await dwcArchiveReader.ReadArchiveAsync(
            archiveReader);
        var observationFactory = GetDwcaObservationFactory(true);
        var processedObservations = dwcObservations
            .Select(m => observationFactory.CreateProcessedObservation(m, false))
            .ToList();
        await AddObservationsToElasticsearchAsync(processedObservations!);
        output.WriteLine($"Processed observations count= {processedObservations.Count}");
    }


    public async Task ImportDwcaFileAsync(string filePath, DataProvider dataProvider, ITestOutputHelper output)
    {
        var parsedDwcaFile = await DwcaHelper.ReadDwcaFileAsync(filePath, dataProvider);
        var observationFactory = GetDwcaObservationFactory(true);
        var eventFactory = GetDwcaEventFactory(true);
        var datasetFactory = GetDwcaDatasetFactory();

        var processedObservations = parsedDwcaFile
            .Occurrences!
            .Select(m => observationFactory.CreateProcessedObservation(m, false))
            .ToList();
        await AddObservationsToElasticsearchAsync(processedObservations!);
        output.WriteLine($"Processed observations count= {processedObservations.Count}");

        var processedEvents = parsedDwcaFile
            .Events!
            .Select(m => eventFactory.CreateEventObservation(m))
            .ToList();
        await AddEventsToElasticsearchAsync(processedEvents);
        output.WriteLine($"Processed events count= {processedEvents.Count}");

        var processedDatasets = parsedDwcaFile
            .Datasets!
            .Select(m => datasetFactory.CreateProcessedDataset(m))
            .ToList();
        await AddDatasetsToElasticsearchAsync(processedDatasets!);
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
                .Occurrences!
                .Select(m => observationFactory.CreateProcessedObservation(m, false))
                .ToList();
            await AddObservationsToElasticsearchAsync(processedObservations!, clearExistingRecords);
            output.WriteLine($"Processed observations count= {processedObservations.Count}");

            var processedEvents = parsedDwcaFile
                .Events!
                .Select(m => eventFactory.CreateEventObservation(m))
                .ToList();
            await AddEventsToElasticsearchAsync(processedEvents, clearExistingRecords);
            output.WriteLine($"Processed events count= {processedEvents.Count}");

            var processedDatasets = parsedDwcaFile
                .Datasets!
                .Select(m => datasetFactory.CreateProcessedDataset(m))
                .ToList();
            await AddDatasetsToElasticsearchAsync(processedDatasets!, clearExistingRecords);
            output.WriteLine($"Processed datasets count= {processedDatasets.Count}");
            clearExistingRecords = false;
        }
    }


    public async Task AddDataToElasticsearchAsync(
        List<Dataset> datasets,
        List<Lib.Models.Processed.DataStewardship.Event.Event> events,
        List<Observation> observations,
        bool clearExistingObservations = true)
    {
        await AddDatasetsToElasticsearchAsync(datasets, clearExistingObservations, 0);
        await AddEventsToElasticsearchAsync(events, clearExistingObservations, 0);
        await AddObservationsToElasticsearchAsync(observations, clearExistingObservations, 0);
        await Task.Delay(1000);
    }

    public async Task AddDataToElasticsearchAsync(
        TestDatas.TestDataSet testDataSet,
        bool clearExistingObservations = true)
    {
        await AddDatasetsToElasticsearchAsync(testDataSet.Datasets!, clearExistingObservations, 0);
        await AddEventsToElasticsearchAsync(testDataSet.Events!, clearExistingObservations, 0);
        await AddObservationsToElasticsearchAsync(testDataSet.Observations!, clearExistingObservations, 0);
        await Task.Delay(1000);
    }

    public async Task AddDataToElasticsearchAsync(
        IEnumerable<Lib.Models.Processed.DataStewardship.Event.Event> events,
        IEnumerable<Observation> observations,
        bool clearExistingObservations = true)
    {
        await AddEventsToElasticsearchAsync(events, clearExistingObservations, 0);
        await AddObservationsToElasticsearchAsync(observations, clearExistingObservations, 0);
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

    public async Task AddObservationsToElasticsearchAsync(IEnumerable<Observation> observations, bool clearExistingObservations = true, int delayInMs = 100)
    {
        var publicObservations = new List<Observation>();
        var protectedObservations = new List<Observation>();

        foreach (var observation in observations)
        {
            if (observation.ShallBeProtected())
            {
                protectedObservations.Add(observation);
            }
            else
            {
                publicObservations.Add(observation);
            }
        }

        await AddObservationsBatchToElasticsearchAsync(publicObservations, false, clearExistingObservations);
        await AddObservationsBatchToElasticsearchAsync(protectedObservations, true, clearExistingObservations);

        await Task.Delay(delayInMs);
    }

    private async Task AddObservationsBatchToElasticsearchAsync(IEnumerable<Observation> observations,
            bool protectedIndex,
            bool clearExistingObservations = true)
    {
        if (clearExistingObservations)
        {
            await _processedObservationCoreRepository.DeleteAllDocumentsAsync(protectedIndex);
        }
        _processedObservationCoreRepository.AddMany(observations, protectedIndex, true);
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

    public async Task ProcessAndAddChecklistsToElasticSearch(IEnumerable<ArtportalenChecklistVerbatim> verbatimChecklists)
    {
        var processedChecklists = ProcessChecklists(verbatimChecklists);
        await AddChecklistsToElasticsearchAsync(processedChecklists);
    }

    public List<Checklist> ProcessChecklists(IEnumerable<ArtportalenChecklistVerbatim> verbatimChecklists)
    {
        var checklists = new List<Checklist>();
        foreach (var verbatimChecklist in verbatimChecklists)
        {
            var checklist = _artportalenChecklistFactory!.CreateProcessedChecklist(verbatimChecklist);
            checklists.Add(checklist!);
        }

        return checklists;
    }

    private async Task AddChecklistsToElasticsearchAsync(IEnumerable<Checklist> checklists, bool clearExistingChecklists = true)
    {
        if (clearExistingChecklists)
        {
            await _processedChecklistRepository.DeleteAllDocumentsAsync();
        }
        await _processedChecklistRepository.DisableIndexingAsync();
        await _processedChecklistRepository.AddManyAsync(checklists);
        await _processedChecklistRepository.EnableIndexingAsync();

        Thread.Sleep(1000);
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
            _taxaById!,
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
