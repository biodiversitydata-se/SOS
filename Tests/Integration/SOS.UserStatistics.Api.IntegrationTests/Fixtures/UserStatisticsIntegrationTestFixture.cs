using SOS.UserStatistics.Api.Cache.Managers;

namespace SOS.UserStatistics.Api.IntegrationTests.Fixtures;

public class UserStatisticsIntegrationTestFixture : FixtureBase, IDisposable
{
    public InstallationEnvironment InstallationEnvironment { get; private set; }
    public UserStatisticsManager UserStatisticsManager { get; set; }
    public IUserStatisticsProcessedObservationRepository UserStatisticsProcessedObservationRepository { get; set; }
    private IUserManager _userManager;
    public string UserAuthenticationToken { get; set; }

    public UserStatisticsIntegrationTestFixture()
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
        Initialize();
    }

    public void Dispose() { }

    protected string GetUserAuthenticationToken()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var userAuthenticationToken = config.GetSection($"{configPrefix}:UserAuthenticationToken").Get<string>();
        return userAuthenticationToken;
    }

    protected MongoDbConfiguration GetMongoDbConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var mongoDbConfiguration = config.GetSection($"{configPrefix}:ProcessDbConfiguration").Get<MongoDbConfiguration>();
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

    private void Initialize()
    {
        UserAuthenticationToken = GetUserAuthenticationToken();
        ElasticSearchConfiguration elasticConfiguration = GetSearchDbConfiguration();

        var elasticClientManager = new ElasticClientManager(elasticConfiguration);
        var mongoDbConfiguration = GetMongoDbConfiguration();
        var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
        var processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
            mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var taxonRepository = new TaxonRepository(processClient, new NullLogger<TaxonRepository>());
        var taxonManager = CreateTaxonManager(processClient, taxonRepository, memoryCache);
        var userStatisticsProcessedObservationRepository = CreateUserStatisticsProcessedObservationRepository(elasticConfiguration, elasticClientManager, processClient);

        var processedConfigurationCache = new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>()));
        var userStatisticsObservationRepository = new UserStatisticsObservationRepository(elasticClientManager, elasticConfiguration, processedConfigurationCache, new NullLogger<UserObservationRepository>());
        var userService = CreateUserService();
        UserStatisticsProcessedObservationRepository = userStatisticsProcessedObservationRepository;
        var areaRepository = new AreaRepository(processClient, new NullLogger<AreaRepository>());
        var areaCache = new AreaCache(areaRepository);
        _userManager = new UserManager(userService, areaCache, new NullLogger<UserManager>());
        var userStatisticsCacheManager = new UserStatisticsCacheManager(new MemoryCache(new MemoryCacheOptions()));
        UserStatisticsManager = new UserStatisticsManager(userStatisticsCacheManager, userStatisticsObservationRepository, userStatisticsProcessedObservationRepository, new NullLogger<UserStatisticsManager>());
    }

    protected virtual IUserService CreateUserService()
    {
        var userServiceConfiguration = GetUserServiceConfiguration();
        var userService = new UserService(new Mock<IAuthorizationProvider>().Object,
            new HttpClientService(new NullLogger<HttpClientService>()), userServiceConfiguration, new NullLogger<UserService>());
        return userService;
    }

    protected virtual IUserService CreateUserService(string token)
    {
        var userServiceConfiguration = GetUserServiceConfiguration();
        IHttpContextAccessor contextAccessor = new HttpContextAccessor();
        contextAccessor.HttpContext = new DefaultHttpContext();
        contextAccessor.HttpContext.Request.Headers.Add("Authorization", token);
        IAuthorizationProvider authorizationProvider = new CurrentUserAuthorization(contextAccessor);
        var userService = new UserService(authorizationProvider,
            new HttpClientService(new NullLogger<HttpClientService>()), userServiceConfiguration, new NullLogger<UserService>());
        return userService;
    }

    private UserStatisticsProcessedObservationRepository CreateUserStatisticsProcessedObservationRepository(
        ElasticSearchConfiguration elasticConfiguration,
        IElasticClientManager elasticClientManager,
        IProcessClient processClient)
    {
        var userStatisticsProcessedObservationRepository = new UserStatisticsProcessedObservationRepository(
            elasticClientManager,
            elasticConfiguration,
            new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>())),
            new NullLogger<ProcessedObservationCoreRepository>());
        return userStatisticsProcessedObservationRepository;
    }

    private TaxonManager CreateTaxonManager(ProcessClient processClient, TaxonRepository taxonRepository, IMemoryCache memoryCache)
    {
        var taxonListRepository = new TaxonListRepository(processClient, new NullLogger<TaxonListRepository>());
        var taxonManager = new TaxonManager(taxonRepository, taxonListRepository,
            new ClassCache<TaxonTree<IBasicTaxon>>(memoryCache),
            new ClassCache<TaxonListSetsById>(memoryCache),
            new NullLogger<TaxonManager>());
        return taxonManager;
    }
}