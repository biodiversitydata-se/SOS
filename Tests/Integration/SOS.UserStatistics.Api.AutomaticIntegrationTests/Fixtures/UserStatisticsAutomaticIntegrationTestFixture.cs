using SOS.Lib.Services;
using SOS.UserStatistics.Api.Repositories.Interfaces;
using Path = System.IO.Path;

namespace SOS.UserStatistics.Api.AutomaticIntegrationTests.Fixtures;

public class UserStatisticsAutomaticIntegrationTestFixture : FixtureBase, IDisposable
{
    public UserStatisticsManager UserStatisticsManager { get; set; }
    public ArtportalenObservationFactory ArtportalenObservationFactory { get; set; }
    public InstallationEnvironment InstallationEnvironment { get; private set; }
    public IUserStatisticsProcessedObservationRepository UserStatisticsProcessedObservationRepository { get; set; }
    public IUserStatisticsObservationRepository UserStatisticsObservationRepository { get; set; }
    public ArtportalenVerbatimRepository ArtportalenVerbatimRepository { get; set; }
    private IUserService _userService;
    private VerbatimClient _importClient;
    public List<Taxon> Taxa;
    private Dictionary<int, Taxon> _taxaById;
    private VocabularyRepository _vocabularyRepository;
    private ProcessClient _processClient;
    private ProcessTimeManager _processTimeManager;
    private VocabularyValueResolver _vocabularyValueResolver;
    public string UserAuthenticationToken { get; set; }
    public TaxonManager TaxonManager { get; private set; }

    private async Task Initialize()
    {
        UserAuthenticationToken = GetUserAuthenticationToken();
        ElasticSearchConfiguration elasticConfiguration = GetSearchDbConfiguration();
        if (!elasticConfiguration.IndexPrefix.Contains("integrationtests"))
            throw new Exception("Elasticsearch configuration must use integrationtest index");
        var observationApiConfiguration = GetObservationApiConfiguration();
        var elasticClientManager = new ElasticClientManager(elasticConfiguration, true);
        var mongoDbConfiguration = GetMongoDbConfiguration();
        var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
        _processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
            mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var taxonRepository = new TaxonRepository(_processClient, new NullLogger<TaxonRepository>());
        bool useTaxonZipCollection = GetUseTaxonZipCollection();
        if (useTaxonZipCollection)
        {
            Taxa = GetTaxaFromZipFile();
        }
        else
        {
            Taxa = await taxonRepository.GetAllAsync();
        }
        _taxaById = Taxa.ToDictionary(m => m.Id, m => m);
        TaxonTree<IBasicTaxon>? basicTaxonTree = TaxonTreeFactory.CreateTaxonTree(Taxa);
        var taxonTreeCache = new ClassCache<TaxonTree<IBasicTaxon>>(memoryCache);
        taxonTreeCache.Set(basicTaxonTree);
        var taxonManager = CreateTaxonManager(_processClient, taxonRepository, memoryCache, taxonTreeCache);

        UserStatisticsObservationRepository = CreateUserStatisticsObservationRepository(elasticConfiguration, elasticClientManager, _processClient);
        UserStatisticsProcessedObservationRepository = CreateUserStatisticsProcessedObservationRepository(elasticConfiguration, elasticClientManager, _processClient, memoryCache, taxonManager);

        _vocabularyRepository = new VocabularyRepository(_processClient, new NullLogger<VocabularyRepository>());
        var processInfoRepository = new ProcessInfoRepository(_processClient, new NullLogger<ProcessInfoRepository>());
        var dataProviderCache = new DataProviderCache(new DataProviderRepository(_processClient, new NullLogger<DataProviderRepository>()));
        _vocabularyValueResolver = new VocabularyValueResolver(_vocabularyRepository, new VocabularyConfiguration { ResolveValues = true, LocalizationCultureCode = "sv-SE" });
        var areaRepository = new AreaRepository(_processClient, new NullLogger<AreaRepository>());
        var areaCache = new AreaCache(areaRepository);
        _userService = CreateUserService();
        var filterManager = new FilterManager(taxonManager, _userService, areaCache, dataProviderCache);
        var userStatisticsManager = new UserStatisticsManager(UserStatisticsObservationRepository, UserStatisticsProcessedObservationRepository, new NullLogger<UserStatisticsManager>());
        TaxonManager = taxonManager;
        var artportalenDataProvider = new DataProvider { Id = 1 };
        _processTimeManager = new ProcessTimeManager(new ProcessConfiguration());
        ArtportalenObservationFactory = await ArtportalenObservationFactory.CreateAsync(
            artportalenDataProvider,
            _taxaById,
            _vocabularyRepository,
            false,
            "https://www.artportalen.se",
            _processTimeManager);
        var verbatimDbConfiguration = GetVerbatimMongoDbConfiguration();
        _importClient = new VerbatimClient(
            verbatimDbConfiguration.GetMongoDbSettings(),
            verbatimDbConfiguration.DatabaseName,
            verbatimDbConfiguration.ReadBatchSize,
            verbatimDbConfiguration.WriteBatchSize);
        ArtportalenVerbatimRepository = new ArtportalenVerbatimRepository(_importClient, new NullLogger<ArtportalenVerbatimRepository>());
        UserStatisticsManager = new UserStatisticsManager(UserStatisticsObservationRepository, UserStatisticsProcessedObservationRepository, new NullLogger<UserStatisticsManager>());
    }

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

        InstallationEnvironment = GetEnvironmentFromAppSettings();
        Initialize().Wait();
        CreateObservationIntegrationTestIndexAsync(false).Wait();
        CreateObservationIntegrationTestIndexAsync(true).Wait();
    }

    public void Dispose()
    {
        DeleteObservationIntegrationTestIndexAsync(false).Wait();
        DeleteObservationIntegrationTestIndexAsync(true).Wait();
    }

    protected string GetUserAuthenticationToken()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var userAuthenticationToken = config.GetSection($"{configPrefix}:UserAuthenticationToken").Get<string>();
        return userAuthenticationToken;
    }

    protected bool GetUseTaxonZipCollection()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var useTaxonZipCollection = config.GetSection($"{configPrefix}:UseTaxonZipCollection").Get<bool>();
        return useTaxonZipCollection;
    }

    protected MongoDbConfiguration GetMongoDbConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var mongoDbConfiguration = config.GetSection($"{configPrefix}:ProcessDbConfiguration").Get<MongoDbConfiguration>();
        return mongoDbConfiguration;
    }

    protected MongoDbConfiguration GetVerbatimMongoDbConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var mongoDbConfiguration = config.GetSection($"{configPrefix}:VerbatimDbConfiguration").Get<MongoDbConfiguration>();
        return mongoDbConfiguration;
    }

    protected ElasticSearchConfiguration GetSearchDbConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var elasticConfiguration = config.GetSection($"{configPrefix}:SearchDbConfiguration").Get<ElasticSearchConfiguration>();
        return elasticConfiguration;
    }

    protected UserServiceConfiguration GetUserServiceConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var userServiceConfiguration = config.GetSection($"{configPrefix}:UserServiceConfiguration").Get<UserServiceConfiguration>();
        return userServiceConfiguration;
    }

    protected ObservationApiConfiguration GetObservationApiConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var observationApiConfiguration = config.GetSection($"{configPrefix}:ObservationApiConfiguration").Get<ObservationApiConfiguration>();
        return observationApiConfiguration;
    }

    private List<Taxon> GetTaxaFromZipFile()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var filePath = Path.Combine(assemblyPath, @"Resources\TaxonCollection.zip");

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
            new ClassCache<TaxonListSetsById>(memoryCache),
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
            new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>())),
            new NullLogger<ProcessedObservationRepository>());
        return userStatisticsProcessedObservationRepository;
    }

    private UserStatisticsObservationRepository CreateUserStatisticsObservationRepository(
        ElasticSearchConfiguration elasticConfiguration,
        IElasticClientManager elasticClientManager,
        IProcessClient processClient)
    {
        var userStatisticsObservationRepository = new UserStatisticsObservationRepository(
            elasticClientManager,
            elasticConfiguration,
            new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>())),
            new NullLogger<UserObservationRepository>());

        return userStatisticsObservationRepository;
    }

    private async Task CreateObservationIntegrationTestIndexAsync(bool protectedIndex)
    {
        await UserStatisticsProcessedObservationRepository.ClearCollectionAsync(protectedIndex);
    }

    private async Task DeleteObservationIntegrationTestIndexAsync(bool protectedIndex)
    {
        await UserStatisticsProcessedObservationRepository.DeleteCollectionAsync(protectedIndex);
    }

    public async Task ProcessAndAddUserObservationToElasticSearch(IEnumerable<ArtportalenObservationVerbatim> verbatimObservations)
    {
        List<Observation> processedObservations = ProcessObservations(verbatimObservations);
        var userObservations = processedObservations.ToUserObservations();
        await AddUserObservationsToElasticsearchAsync(userObservations);
    }

    public async Task AddUserObservationsToElasticsearchAsync(IEnumerable<UserObservation> userObservations, bool clearExistingUserObservations = true)
    {
        if (clearExistingUserObservations)
        {
            await UserStatisticsObservationRepository.DeleteAllDocumentsAsync();
        }
        await UserStatisticsObservationRepository.DisableIndexingAsync();
        await UserStatisticsObservationRepository.AddManyAsync(userObservations);
        await UserStatisticsObservationRepository.EnableIndexingAsync();

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