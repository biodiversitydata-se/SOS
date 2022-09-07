namespace SOS.UserStatistics.Api.Tests;

public class UserStatisticsTestFixture : FixtureBase, IDisposable
{
    public UserStatisticsTestFixture()
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
    }

    public InstallationEnvironment InstallationEnvironment { get; private set; }
    public UserStatisticsModule UserStatisticsModule { get; private set; }
    public IUserStatisticsProcessedObservationRepository UserStatisticsProcessedObservationRepository { get; set; }

    private IFilterManager _filterManager;
    private IUserManager _userManager;
    public string UserAuthenticationToken { get; set; }

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

    protected ElasticSearchConfiguration GetCustomSearchDbConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var elasticConfiguration = config.GetSection($"{configPrefix}:CustomSearchDbConfiguration").Get<ElasticSearchConfiguration>();
        return elasticConfiguration;
    }

    protected UserServiceConfiguration GetUserServiceConfiguration()
    {
        var config = GetAppSettings();
        var configPrefix = GetConfigPrefix(InstallationEnvironment);
        var userServiceConfiguration = config.GetSection($"{configPrefix}:UserServiceConfiguration").Get<UserServiceConfiguration>();
        return userServiceConfiguration;
    }

    private async Task Initialize()
    {
        UserAuthenticationToken = GetUserAuthenticationToken();
        ElasticSearchConfiguration elasticConfiguration = GetSearchDbConfiguration();

        var elasticClientManager = new ElasticClientManager(elasticConfiguration, true);
        var mongoDbConfiguration = GetMongoDbConfiguration();
        var processedSettings = mongoDbConfiguration.GetMongoDbSettings();
        var processClient = new ProcessClient(processedSettings, mongoDbConfiguration.DatabaseName,
            mongoDbConfiguration.ReadBatchSize, mongoDbConfiguration.WriteBatchSize);

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var taxonRepository = new TaxonRepository(processClient, new NullLogger<TaxonRepository>());
        var taxonManager = CreateTaxonManager(processClient, taxonRepository, memoryCache);
        var userStatisticsProcessedObservationRepository = CreateUserStatisticsProcessedObservationRepository(elasticConfiguration, elasticClientManager, processClient, memoryCache);
        var processedTaxonRepository = CreateProcessedTaxonRepository(elasticConfiguration, elasticClientManager, processClient, taxonManager);


        var areaRepository = new AreaRepository(processClient, new NullLogger<AreaRepository>());
        var areaCache = new AreaCache(areaRepository);
        var userService = CreateUserService();
        UserStatisticsProcessedObservationRepository = userStatisticsProcessedObservationRepository;
        ElasticSearchConfiguration customElasticConfiguration = GetCustomSearchDbConfiguration();


        _userManager = new UserManager(userService, new NullLogger<UserManager>());

        var artportalenDataProvider = new Lib.Models.Shared.DataProvider { Id = 1 };
        var taxa = await taxonRepository.GetAllAsync();
        var taxaById = taxa.ToDictionary(m => m.Id, m => m);
        var processedConfigurationCache = new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>()));
        var userObservationRepository = new UserObservationRepository(elasticClientManager, elasticConfiguration, processedConfigurationCache, new NullLogger<UserObservationRepository>());
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
        IProcessClient processClient,
        IMemoryCache memoryCache)
    {
        var processedConfigurationCache = new ClassCache<ProcessedConfiguration>(memoryCache);
        var userStatisticsProcessedObservationRepository = new UserStatisticsProcessedObservationRepository(
            elasticClientManager,
            elasticConfiguration,
            new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>())),
            new NullLogger<ProcessedObservationRepository>());
        return userStatisticsProcessedObservationRepository;
    }

    private ProcessedTaxonRepository CreateProcessedTaxonRepository(
        ElasticSearchConfiguration elasticConfiguration,
        IElasticClientManager elasticClientManager,
        IProcessClient processClient,
        ITaxonManager taxonManager)
    {
        var processedTaxonRepository = new ProcessedTaxonRepository(
            elasticClientManager,
            elasticConfiguration,
            new ProcessedConfigurationCache(new ProcessedConfigurationRepository(processClient, new NullLogger<ProcessedConfigurationRepository>())),
            new HttpContextAccessor(),
            taxonManager,
            new NullLogger<ProcessedTaxonRepository>());
        return processedTaxonRepository;
    }

    public void UseMockUserService(params AuthorityModel[] authorities)
    {
        UserModel user = new UserModel();
        user.Id = 15;
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(userService => userService.GetUserAsync())
            .ReturnsAsync(user);
        userServiceMock.Setup(userService =>
                userService.GetUserAuthoritiesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(authorities);
        _filterManager.UserService = userServiceMock.Object;

        var contextAccessor = new HttpContextAccessor() { HttpContext = new DefaultHttpContext() };
        var claimsIdentity = new ClaimsIdentity();
        var claim = new Claim("scope", "SOS.Observations.Protected");
        claimsIdentity.AddClaim(claim);
        contextAccessor.HttpContext.User.AddIdentity(claimsIdentity);
        UserStatisticsProcessedObservationRepository.HttpContextAccessor = contextAccessor;
    }


    public void UseUserServiceWithToken(string token)
    {
        var userService = CreateUserService(token);
        _filterManager.UserService = userService;
        _userManager.UserService = userService;
    }
}