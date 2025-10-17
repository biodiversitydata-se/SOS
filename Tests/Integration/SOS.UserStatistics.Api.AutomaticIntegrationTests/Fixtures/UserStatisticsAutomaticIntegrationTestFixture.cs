﻿using SOS.UserStatistics.Api.Cache.Managers;
using SOS.UserStatistics.Api.Cache.Managers.Interfaces;
using SOS.UserStatistics.Api.Configuration;
using SOS.Harvest.Extensions;
using SOS.Lib.Models.Processed.Configuration;
using System.Collections.Concurrent;
using Elastic.Clients.Elasticsearch.Cluster;

namespace SOS.UserStatistics.Api.AutomaticIntegrationTests.Fixtures;

public class UserStatisticsAutomaticIntegrationTestFixture : FixtureBase, IDisposable
{
    private Dictionary<int, Taxon> _taxaById;
    private VocabularyRepository _vocabularyRepository;
    private ProcessClient _processClient;
    private ProcessTimeManager _processTimeManager;
    private VocabularyValueResolver _vocabularyValueResolver;
    private ArtportalenObservationFactory ArtportalenObservationFactory { get; set; }
    private InstallationEnvironment _installationEnvironment { get; set; }
    private IUserStatisticsCacheManager _userStatisticsCacheManager { get; set; }
    private IUserStatisticsProcessedObservationRepository _userStatisticsProcessedObservationRepository { get; set; }
    private IUserStatisticsObservationRepository _userStatisticsObservationRepository { get; set; }
    private List<Taxon> _taxa { get; set; }
    public UserStatisticsManager UserStatisticsManager { get; set; }

    public UserStatisticsAutomaticIntegrationTestFixture()
    {
        // MongoDB conventions.
        ConventionRegistry.Register(
            "MongoDB Solution Conventions",
            new ConventionPack
            {
                    new IgnoreExtraElementsConvention(true),
                    new IgnoreIfNullConvention(true)
            },
            t => true);

        _installationEnvironment = GetEnvironmentFromAppSettings();
        InitializeAsync().Wait();
        CreateObservationIntegrationTestIndexAsync(false).Wait();
        CreateObservationIntegrationTestIndexAsync(true).Wait();
        CreateUserObservationIntegrationTestIndexAsync().Wait();
    }

    private async Task InitializeAsync()
    {
        ElasticSearchConfiguration elasticConfiguration = GetSearchDbConfiguration();
        if (!elasticConfiguration.IndexPrefix.Contains("integrationtests"))
            throw new Exception("Elasticsearch configuration must use integrationtest index");
        var userStatisticsApiConfiguration = GetUserStatisticsApiConfiguration();
        var elasticClientManager = new ElasticClientManager(elasticConfiguration);
        var mongoDbConfiguration = GetMongoDbConfiguration();
        var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
        _processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
            mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);
        var datasetRepository = new ArtportalenDatasetMetadataRepository(_processClient, new NullLogger<ArtportalenDatasetMetadataRepository>());
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var taxonRepository = new TaxonRepository(_processClient, new NullLogger<TaxonRepository>());
        _taxa = GetUseTaxonZipCollection() ? GetTaxaFromZipFile() : await taxonRepository.GetAllAsync();
        _taxaById = _taxa.ToDictionary(m => m.Id, m => m);
        var basicTaxonTree = TaxonTreeFactory.CreateTaxonTree(_taxaById);
        var taxonTreeCache = new ClassCache<TaxonTree<IBasicTaxon>>(memoryCache, new NullLogger<ClassCache<TaxonTree<IBasicTaxon>>>());
        taxonTreeCache.Set(basicTaxonTree);
        var taxonManager = CreateTaxonManager(_processClient, taxonRepository, memoryCache, taxonTreeCache);

        _userStatisticsCacheManager = new UserStatisticsCacheManager(new MemoryCache(new MemoryCacheOptions()));
        _userStatisticsObservationRepository = CreateUserStatisticsObservationRepository(elasticConfiguration, elasticClientManager, memoryCache, _processClient);
        _userStatisticsProcessedObservationRepository = CreateUserStatisticsProcessedObservationRepository(elasticConfiguration, elasticClientManager, _processClient, memoryCache, taxonManager);

        _vocabularyRepository = new VocabularyRepository(_processClient, new NullLogger<VocabularyRepository>());
        _vocabularyValueResolver = new VocabularyValueResolver(_vocabularyRepository, new VocabularyConfiguration { ResolveValues = true, LocalizationCultureCode = "sv-SE" });
        var processConfiguration = new ProcessConfiguration();
        _processTimeManager = new ProcessTimeManager(processConfiguration);
        ArtportalenObservationFactory = await ArtportalenObservationFactory.CreateAsync(
            new DataProvider { Id = 1 },
            _taxaById,
            _vocabularyRepository,
            datasetRepository,
            false,
            "https://www.artportalen.se",
            _processTimeManager,
            processConfiguration);

        UserStatisticsManager = new UserStatisticsManager(_userStatisticsCacheManager, _userStatisticsObservationRepository, _userStatisticsProcessedObservationRepository, new NullLogger<UserStatisticsManager>());
    }

    public void Dispose()
    {
        DeleteObservationIntegrationTestIndexAsync(false).Wait();
        DeleteObservationIntegrationTestIndexAsync(true).Wait();
    }

    protected bool GetUseTaxonZipCollection()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(_installationEnvironment);
        var useTaxonZipCollection = config.GetSection($"{configPrefix}:UseTaxonZipCollection").Get<bool>();
        return useTaxonZipCollection;
    }

    protected MongoDbConfiguration GetMongoDbConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(_installationEnvironment);
        var mongoDbConfiguration = config.GetSection($"{configPrefix}:ProcessDbConfiguration").Get<MongoDbConfiguration>();
        return mongoDbConfiguration;
    }

    protected ElasticSearchConfiguration GetSearchDbConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(_installationEnvironment);
        var elasticConfiguration = config.GetSection($"{configPrefix}:SearchDbConfiguration").Get<ElasticSearchConfiguration>();
        return elasticConfiguration;
    }

    protected UserServiceConfiguration GetUserServiceConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(_installationEnvironment);
        var userServiceConfiguration = config.GetSection($"{configPrefix}:UserServiceConfiguration").Get<UserServiceConfiguration>();
        return userServiceConfiguration;
    }

    protected UserStatisticsApiConfiguration GetUserStatisticsApiConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(_installationEnvironment);
        var observationApiConfiguration = config.GetSection($"{configPrefix}:UserStatisticsApiConfiguration").Get<UserStatisticsApiConfiguration>();
        return observationApiConfiguration;
    }

    private List<Taxon> GetTaxaFromZipFile()
    {
        var assemblyPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var filePath = System.IO.Path.Combine(assemblyPath, @"Resources\TaxonCollection.zip");

        using (ZipArchive archive = ZipFile.OpenRead(filePath))
        {
            var taxonFile = archive.Entries.FirstOrDefault(f =>
                f.Name.Equals("TaxonCollection.json", StringComparison.CurrentCultureIgnoreCase));

            var taxonFileStream = taxonFile.Open();
            using var sr = new StreamReader(taxonFileStream, Encoding.UTF8);
            string strJson = sr.ReadToEnd();
            var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            var taxa = System.Text.Json.JsonSerializer.Deserialize<List<Taxon>>(strJson, jsonSerializerOptions);
            return taxa;
        }
    }

    private TaxonManager CreateTaxonManager(ProcessClient processClient,
        TaxonRepository taxonRepository,
        IMemoryCache memoryCache,
        IClassCache<TaxonTree<IBasicTaxon>> taxonTreeCache)
    {
        var taxonListRepository = new TaxonListRepository(processClient, new NullLogger<TaxonListRepository>());
        var taxonManager = new TaxonManager(taxonRepository, taxonListRepository,
            taxonTreeCache,
            new ClassCache<TaxonListSetsById>(memoryCache, new NullLogger<ClassCache<TaxonListSetsById>>()),
            new NullLogger<TaxonManager>());
        return taxonManager;
    }

    protected virtual IUserService CreateUserService()
    {
        var userServiceConfiguration = GetUserServiceConfiguration();
        var userService = new UserService(new Mock<IAuthorizationProvider>().Object,
            new HttpClientService(new NullLogger<HttpClientService>()), userServiceConfiguration, new NullLogger<UserService>());
        return userService;
    }

    private UserStatisticsProcessedObservationRepository CreateUserStatisticsProcessedObservationRepository(
        ElasticSearchConfiguration elasticConfiguration,
        IElasticClientManager elasticClientManager,
        IProcessClient processClient,
        IMemoryCache memoryCache,
        ITaxonManager taxonManager)
    {
        var userStatisticsProcessedObservationRepository = new UserStatisticsProcessedObservationRepository(
            elasticClientManager,
            elasticConfiguration,
            new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>()), new NullLogger<CacheBase<string, ProcessedConfiguration>>()),
            taxonManager,
            new ClassCache<ConcurrentDictionary<string, HealthResponse>>(memoryCache, new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()),
            memoryCache,
            new NullLogger<UserStatisticsProcessedObservationRepository>());
        return userStatisticsProcessedObservationRepository;
    }

    private UserStatisticsObservationRepository CreateUserStatisticsObservationRepository(
        ElasticSearchConfiguration elasticConfiguration,
        IElasticClientManager elasticClientManager,
        IMemoryCache memoryCache,
        IProcessClient processClient)
    {
        var userStatisticsObservationRepository = new UserStatisticsObservationRepository(
            elasticClientManager,
            elasticConfiguration,
            new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>()), new NullLogger<CacheBase<string, ProcessedConfiguration>>()),
            new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()),
            memoryCache,
            new NullLogger<UserObservationRepository>());

        return userStatisticsObservationRepository;
    }

    private async Task CreateObservationIntegrationTestIndexAsync(bool protectedIndex)
    {
        await _userStatisticsProcessedObservationRepository.ClearCollectionAsync(protectedIndex);
    }

    private async Task DeleteObservationIntegrationTestIndexAsync(bool protectedIndex)
    {
        await _userStatisticsProcessedObservationRepository.DeleteCollectionAsync(protectedIndex);
    }

    private async Task CreateUserObservationIntegrationTestIndexAsync()
    {
        await _userStatisticsObservationRepository.ClearCollectionAsync();
    }

    public async Task ProcessAndAddObservationsToElasticSearch(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
    {
        var processedObservations = ProcessObservations(verbatimObservations);
        await AddObservationsToElasticsearchAsync(processedObservations);
    }

    public async Task ProcessAndAddUserObservationToElasticSearch(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
    {
        List<Observation> processedObservations = ProcessObservations(verbatimObservations);
        var userObservations = processedObservations.ToUserObservations();
        await AddUserObservationsToElasticsearchAsync(userObservations);
    }

    public async Task AddObservationsToElasticsearchAsync(IEnumerable<Observation> observations, bool clearExistingObservations = true)
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

        Thread.Sleep(1000);
    }

    private async Task AddObservationsBatchToElasticsearchAsync(IEnumerable<Observation> observations,
           bool protectedIndex,
           bool clearExistingObservations = true)
    {
        if (clearExistingObservations)
        {
            await _userStatisticsProcessedObservationRepository.DeleteAllDocumentsAsync(protectedIndex);
        }
        await _userStatisticsProcessedObservationRepository.DisableIndexingAsync(protectedIndex);
        await _userStatisticsProcessedObservationRepository.AddManyAsync(observations, protectedIndex);
        await _userStatisticsProcessedObservationRepository.EnableIndexingAsync(protectedIndex);
    }

    public async Task AddUserObservationsToElasticsearchAsync(IEnumerable<UserObservation> userObservations, bool clearExistingUserObservations = true)
    {
        if (clearExistingUserObservations)
        {
            await _userStatisticsObservationRepository.DeleteAllDocumentsAsync();
        }
        await _userStatisticsObservationRepository.DisableIndexingAsync();
        await _userStatisticsObservationRepository.AddManyAsync(userObservations);
        await _userStatisticsObservationRepository.EnableIndexingAsync();

        Thread.Sleep(1000);
    }

    public List<Observation> ProcessObservations(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
    {
        var processedObservations = new List<Observation>();
        bool diffuseIfSupported = false;
        foreach (var verbatimObservation in verbatimObservations)
        {
            var processedObservation = ArtportalenObservationFactory.CreateProcessedObservation(verbatimObservation, diffuseIfSupported);
            processedObservations.Add(processedObservation);
        }

        _vocabularyValueResolver.ResolveVocabularyMappedValues(processedObservations, true);
        return processedObservations;
    }
}