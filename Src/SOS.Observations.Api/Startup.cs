﻿using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using HealthChecks.UI.Client;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Nest;
using Newtonsoft.Json.Converters;
using SOS.Lib.ActionFilters;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.DwcArchive;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.JsonConverters;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Middleware;
using SOS.Lib.Models.Cache;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Swagger;
using SOS.Observations.Api.ApplicationInsights;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.HealthChecks;
using SOS.Observations.Api.HealthChecks.Custom;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Middleware;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Observations.Api.Services.Interfaces;
using SOS.Shared.Api.Configuration;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Utilities.Objects;
using SOS.Shared.Api.Utilities.Objects.Interfaces;
using SOS.Shared.Api.Validators;
using SOS.Shared.Api.Validators.Interfaces;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DataProviderManager = SOS.Observations.Api.Managers.DataProviderManager;
using IDataProviderManager = SOS.Observations.Api.Managers.Interfaces.IDataProviderManager;

namespace SOS.Observations.Api
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Startup
    {
        private const string InternalApiName = "InternalSosObservations";
        private const string PublicApiName = "PublicSosObservations";
        private const string AzureInternalApiName = "AzureInternalSosObservations";
        private const string AzurePublicApiName = "AzurePublicSosObservations";
        private const string InternalApiPrefix = "Internal";
        private const string AzureApiPrefix = "Azure";
        private IWebHostEnvironment CurrentEnvironment { get; set; }
        private bool _isDevelopment;
        private bool _disableHangfireInit = false;
        private bool _disableHealthCheckInit = false;
        private bool _disableCachedTaxonSumAggregationInit = false;

        private class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
        {
            readonly IApiVersionDescriptionProvider provider;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) =>
            this.provider = provider;

            public void Configure(SwaggerGenOptions options)
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(
                           $"{InternalApiName}{description.GroupName}",
                           new OpenApiInfo()
                           {
                               Title = $"SOS Observations API (Internal) {description.GroupName.ToUpperInvariant()}",
                               Version = description.ApiVersion.ToString(),
                               Description = "Species Observation System (SOS) - Observations API. Internal API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                           });
                    options.SwaggerDoc(
                        $"{PublicApiName}{description.GroupName}",
                        new OpenApiInfo()
                        {
                            Title = $"SOS Observations API (Public) {description.GroupName.ToUpperInvariant()}",
                            Version = description.ApiVersion.ToString(),
                            Description = "Species Observation System (SOS) - Observations API. Public API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                        });
                    options.SwaggerDoc(
                           $"{AzureInternalApiName}{description.GroupName}",
                           new OpenApiInfo()
                           {
                               Title = $"SOS Observations API (Internal - Azure) {description.GroupName.ToUpperInvariant()}",
                               Version = description.ApiVersion.ToString(),
                               Description = "Species Observation System (SOS) - Observations API. Internal - Azure API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                           });
                    options.SwaggerDoc(
                        $"{AzurePublicApiName}{description.GroupName}",
                        new OpenApiInfo()
                        {
                            Title = $"SOS Observations API (Public - Azure) {description.GroupName.ToUpperInvariant()}",
                            Version = description.ApiVersion.ToString(),
                            Description = "Species Observation System (SOS) - Observations API. Public - Azure API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                        });
                    //var schemaHelper = new SwashbuckleSchemaHelper();
                    //swagger.CustomSchemaIds(type => schemaHelper.GetSchemaId(type)); // temporarily used when checking for schema duplicates.
                    options.CustomOperationIds(apiDesc =>
                    {
                        apiDesc.TryGetMethodInfo(out MethodInfo methodInfo);
                        string controller = apiDesc.ActionDescriptor.RouteValues["controller"];
                        string methodName = methodInfo.Name;

                        return $"{controller}_{methodName}".Replace("Async", "", StringComparison.InvariantCultureIgnoreCase);
                    });
                }
            }
        }

        /// <summary>
        ///     Start up
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            var environment = env.EnvironmentName.ToLower();
            CurrentEnvironment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables();

            _isDevelopment = CurrentEnvironment.IsEnvironment("local") || CurrentEnvironment.IsEnvironment("dev") || CurrentEnvironment.IsEnvironment("st");
            _disableHangfireInit = GetDisableFeature(environmentVariable: "DISABLE_HANGFIRE_INIT");
            _disableHealthCheckInit = GetDisableFeature(environmentVariable: "DISABLE_HEALTHCHECK_INIT");
            _disableCachedTaxonSumAggregationInit = GetDisableFeature(environmentVariable: "DISABLE_CACHED_TAXON_SUM_INIT");

            // Add user secrets if debug or if development mode.            
            // (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
            // In production you should store the secret values as environment variables.
#if DEBUG
            builder.AddUserSecrets<Startup>();
#else
            if (_isDevelopment)
            {                
                builder.AddUserSecrets<Startup>();
            }
#endif

            Configuration = builder.Build();
        }

        /// <summary>
        ///     Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Use Swedish culture info.
            var culture = new CultureInfo("sv-SE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            var applicationInsightsConfiguration = Configuration.GetSection("ApplicationInsights").Get<Lib.Configuration.Shared.ApplicationInsights>();
            services.AddSingleton(applicationInsightsConfiguration);

            // Add application insights.
            services.AddApplicationInsightsTelemetry(c =>
            {
                c.ConnectionString = applicationInsightsConfiguration.ConnectionString;
            });
            // Application insights custom
            services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

            services.AddMemoryCache();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.AllowTrailingCommas = true;
                    //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // Some clients does not support omitting null values, so use JsonIgnoreCondition.Never for now.
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new GeoShapeConverter());
                    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()); // Is this needed?
                });

            // MongoDB conventions.
            ConventionRegistry.Register(
                "MongoDB Solution Conventions",
                new ConventionPack
                {
                    new IgnoreExtraElementsConvention(true),
                    new IgnoreIfNullConvention(true)
                },
                t => true);

            // Identity service configuration
            var identityServerConfiguration = Configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();
            var userServiceConfiguration = Configuration.GetSection("UserServiceConfiguration").Get<UserServiceConfiguration>();

            // Authentication
            services.AddAuthentication(options =>
            {                
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("UserAdmin2", options =>
            {
                options.Audience = userServiceConfiguration.IdentityProvider.Audience;
                options.Authority = userServiceConfiguration.IdentityProvider.Authority;
                options.RequireHttpsMetadata = userServiceConfiguration.IdentityProvider.RequireHttpsMetadata;
                options.TokenValidationParameters.RoleClaimType = "rname";
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        var scopeClaim = claimsIdentity?.FindFirst("scope");
                        if (claimsIdentity != null && scopeClaim != null)
                        {
                            var scopes = scopeClaim.Value.Split(' ');
                            claimsIdentity.RemoveClaim(scopeClaim);
                            foreach (var scope in scopes)
                            {
                                claimsIdentity.AddClaim(new Claim("scope", scope));
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer("UserAdmin1", options =>
            {
                options.Audience = identityServerConfiguration.Audience;
                options.Authority = identityServerConfiguration.Authority;
                options.RequireHttpsMetadata = identityServerConfiguration.RequireHttpsMetadata;
                options.TokenValidationParameters.RoleClaimType = "rname";
            });
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("MultipleIdentityProviders", policy =>
                {                    
                    policy.RequireAuthenticatedUser();
                    policy.AddAuthenticationSchemes("UserAdmin1", "UserAdmin2");
                });
            });

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 5);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
            }).AddApiExplorer(options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VV";
                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(
                options =>
                {
                    // add a custom operation filters
                    options.OperationFilter<SwaggerDefaultValues>();
                    options.OperationFilter<SwaggerAddOptionalHeaderParameters>();

                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var xmlDocs = currentAssembly.GetReferencedAssemblies()
                        .Union(new AssemblyName[] { currentAssembly.GetName() })
                        .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                        .Where(f => File.Exists(f)).ToArray();

                    Array.ForEach(xmlDocs, (d) =>
                    {
                        options.IncludeXmlComments(d);
                    });

                    options.SchemaFilter<SwaggerIgnoreFilter>();
                    options.SchemaFilter<SwaggerForceSchemaFilter>();
                    // Post-modify Operation descriptions once they've been generated by wiring up one or more
                    // Operation filters.
                    options.OperationFilter<ApiManagementDocumentationFilter>();

                    options.DocInclusionPredicate((documentName, apiDescription) =>
                    {
                        var apiVersions = GetApiVersions(apiDescription);
                        var versionMatch = apiVersions.Any(v =>
                                   $"{InternalApiName}v{v}" == documentName) ||
                               apiVersions.Any(v =>
                                   $"{PublicApiName}v{v}" == documentName) ||
                               apiVersions.Any(v =>
                                   $"{AzureInternalApiName}v{v}" == documentName) ||
                               apiVersions.Any(v =>
                                   $"{AzurePublicApiName}v{v}" == documentName);

                        var isAzurePublicApi = apiDescription.ActionDescriptor.EndpointMetadata.Any(x => x.GetType() == typeof(AzureApiAttribute));
                        var isAzurePublicDocument = documentName.Contains(AzureApiPrefix, StringComparison.CurrentCultureIgnoreCase) && !documentName.Contains(InternalApiPrefix, StringComparison.CurrentCultureIgnoreCase);
                        var isAzureInternalApi = apiDescription.ActionDescriptor.EndpointMetadata.Any(x => x.GetType() == typeof(AzureInternalApiAttribute));
                        var isAzureInternalDocument = documentName.Contains(AzureApiPrefix, StringComparison.CurrentCultureIgnoreCase) && documentName.Contains(InternalApiPrefix, StringComparison.CurrentCultureIgnoreCase);
                        var isInternalApi = apiDescription.ActionDescriptor.EndpointMetadata.Any(x => x.GetType() == typeof(InternalApiAttribute));
                        var isInternalDocument = documentName.Contains(InternalApiPrefix, StringComparison.CurrentCultureIgnoreCase) && !documentName.Contains(AzureApiPrefix, StringComparison.CurrentCultureIgnoreCase);

                        return
                            versionMatch && (
                                (isAzurePublicApi && isAzurePublicDocument) ||
                                (isAzureInternalApi && isAzureInternalDocument)
                            ) || (
                            !isAzurePublicDocument && !isAzureInternalDocument &&
                            (isInternalDocument || !isInternalApi)
                        );
                    });

                    options.AddSecurityDefinition("Bearer", //Name the security scheme
                        new OpenApiSecurityScheme
                        {
                            Name = "Authorization",
                            Description = "JWT Authorization header using the Bearer scheme.",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
                            Scheme = "bearer" //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                        });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement{
                        {
                            new OpenApiSecurityScheme{
                                Scheme = "bearer",
                                Name = "Bearer",
                                In = ParameterLocation.Header,
                                Reference = new OpenApiReference{
                                    Id = "Bearer", //The name of the previously defined security scheme.
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            new List<string>()
                        }
                    });
                });

            var observationApiConfiguration = Configuration.GetSection("ObservationApiConfiguration")
                .Get<ObservationApiConfiguration>();

            // Response compression
            if (observationApiConfiguration.EnableResponseCompression)
            {
                services.AddResponseCompression(o => o.EnableForHttps = true);
                services.Configure<BrotliCompressionProviderOptions>(options =>
                {
                    options.Level = observationApiConfiguration.ResponseCompressionLevel;
                });
                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = observationApiConfiguration.ResponseCompressionLevel;
                });
            }

            // Hangfire
            if (!_disableHangfireInit)
            {
                var mongoConfiguration = Configuration.GetSection("HangfireDbConfiguration").Get<HangfireDbConfiguration>();
                services.AddHangfire(configuration =>
                    configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings(m =>
                        {
                            m.Converters.Add(new NewtonsoftGeoShapeConverter());
                            m.Converters.Add(new StringEnumConverter());
                        })
                        .UseMongoStorage(new MongoClient(mongoConfiguration.GetMongoDbSettings()),
                            mongoConfiguration.DatabaseName,
                            new MongoStorageOptions
                            {
                                MigrationOptions = new MongoMigrationOptions
                                {
                                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                                    BackupStrategy = new CollectionMongoBackupStrategy()
                                },
                                Prefix = "hangfire",
                                CheckConnection = true
                            })
                );
            }

            services.AddSingleton(Configuration.GetSection("CryptoConfiguration").Get<CryptoConfiguration>());

            //setup the elastic search configuration
            var elasticConfiguration = Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
            services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));

            // Processed Mongo Db
            var processedDbConfiguration = Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

            var blobStorageConfiguration = Configuration.GetSection("BlobStorageConfiguration")
                .Get<BlobStorageConfiguration>();

            var healthCheckConfiguration = Configuration.GetSection("HealthCheckConfiguration").Get<HealthCheckConfiguration>();
            var artportalenApiServiceConfiguration = Configuration.GetSection("ArtportalenApiServiceConfiguration").Get<ArtportalenApiServiceConfiguration>();

            // Add configuration
            services.AddSingleton(artportalenApiServiceConfiguration);
            services.AddSingleton(observationApiConfiguration);
            services.AddSingleton(blobStorageConfiguration);
            services.AddSingleton(Configuration.GetSection("DevOpsConfiguration").Get<DevOpsConfiguration>());
            services.AddSingleton(elasticConfiguration);
            services.AddSingleton(Configuration.GetSection("InputValaidationConfiguration").Get<InputValaidationConfiguration>()!);
            services.AddSingleton(Configuration.GetSection("UserServiceConfiguration").Get<UserServiceConfiguration>());
            services.AddSingleton(healthCheckConfiguration);
            services.AddSingleton(Configuration.GetSection("VocabularyConfiguration").Get<VocabularyConfiguration>());
            services.AddSingleton(Configuration);
            services.AddSingleton(Configuration.GetSection("AreaConfiguration").Get<AreaConfiguration>());


#if !DEBUG
            if (!_disableHealthCheckInit)
            {
                services.AddSingleton<IHealthCheckPublisher, HealthReportCachePublisher>();  
                services.Configure<HealthCheckPublisherOptions>(options => {
                    options.Delay = TimeSpan.FromSeconds(10);
                    options.Period = TimeSpan.FromSeconds(600); // Create new health check every 10 minutes and cache result
                    options.Timeout = TimeSpan.FromSeconds(60);
                });
                var healthChecks = services.AddHealthChecks()
                    .AddDiskStorageHealthCheck(
                        x => x.AddDrive("C:\\", (long)(healthCheckConfiguration.MinimumLocalDiskStorage * 1000)),
                        name: $"Primary disk: min {healthCheckConfiguration.MinimumLocalDiskStorage}GB free - warning",
                        failureStatus: HealthStatus.Degraded,
                        tags: new[] { "disk" })
                    .AddMongoDb(processedDbConfiguration.GetConnectionString(), tags: new[] { "database", "mongodb" })
                    .AddHangfire(a => a.MinimumAvailableServers = 1, "Hangfire", tags: new[] { "hangfire" })
                    .AddCheck<DataAmountHealthCheck>("Data amount", tags: new[] { "database", "elasticsearch", "data" })
                    .AddCheck<SearchDataProvidersHealthCheck>("Search data providers", tags: new[] { "database", "elasticsearch", "query" })
                    //.AddCheck<SearchPerformanceHealthCheck>("Search performance", tags: new[] { "database", "elasticsearch", "query", "performance" })
                    .AddCheck<AzureSearchHealthCheck>("Azure search API health check", tags: new[] { "azure", "database", "elasticsearch", "query" })
                    .AddCheck<DataProviderHealthCheck>("Data providers", tags: new[] { "data providers", "meta data" })
                    .AddCheck<ElasticsearchProxyHealthCheck>("ElasticSearch Proxy", tags: new[] { "wfs", "elasticsearch" })
                    //.AddCheck<DuplicateHealthCheck>("Duplicate observations", tags: new[] { "elasticsearch", "harvest" })
                    .AddCheck<ElasticsearchHealthCheck>("Elasticsearch", tags: new[] { "database", "elasticsearch" })
                    .AddCheck<DependenciesHealthCheck>("Dependencies", tags: new[] { "dependencies" })
                    .AddCheck<APDbRestoreHealthCheck>("Artportalen database backup restore", tags: new[] { "database", "sql server" });

                if (CurrentEnvironment.IsEnvironment("prod"))
                {
                    healthChecks.AddCheck<DwcaHealthCheck>("DwC-A files", tags: new[] { "dwca", "export" });
                    healthChecks.AddCheck<ApplicationInsightstHealthCheck>("Application Insights", tags: new[] { "application insights", "harvest" });
                    healthChecks.AddCheck<WFSHealthCheck>("WFS", tags: new[] { "wfs" }); // add this to ST environment when we have a GeoServer test environment.
                }
            }
#endif

            // Add security
            services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();

            // Add Caches
            services.AddSingleton<IAreaCache, AreaCache>();
            services.AddSingleton<ITaxonObservationCountCache, TaxonObservationCountCache>();
            services.AddSingleton<IDataProviderCache, DataProviderCache>();
            services.AddSingleton<ICache<int, ProjectInfo>, ProjectCache>();
            services.AddSingleton<ICache<VocabularyId, Vocabulary>, VocabularyCache>();
            services.AddSingleton<ICache<int, TaxonList>, TaxonListCache>();
            services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
            services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
            services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
            var taxonSumAggregationCache = new ClassCache<Dictionary<int, TaxonSumAggregationItem>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<int, TaxonSumAggregationItem>>>()) { CacheDuration = TimeSpan.FromHours(48) };
            services.AddSingleton<IClassCache<Dictionary<int, TaxonSumAggregationItem>>>(taxonSumAggregationCache);
            var geoGridAggregationCache = new ClassCache<Dictionary<string, CacheEntry<GeoGridResultDto>>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<string, CacheEntry<GeoGridResultDto>>>>()) { CacheDuration = TimeSpan.FromHours(48) };
            services.AddSingleton<IClassCache<Dictionary<string, CacheEntry<GeoGridResultDto>>>>(geoGridAggregationCache);
            var taxonAggregationInternalCache = new ClassCache<Dictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>>>()) { CacheDuration = TimeSpan.FromHours(1) };
            services.AddSingleton<IClassCache<Dictionary<string, CacheEntry<PagedResultDto<TaxonAggregationItemDto>>>>>(taxonAggregationInternalCache);
            var clusterHealthCache = new ClassCache<Dictionary<string, ClusterHealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<Dictionary<string, ClusterHealthResponse>>>()) { CacheDuration = TimeSpan.FromMinutes(2) };
            services.AddSingleton<IClassCache<Dictionary<string, ClusterHealthResponse>>>(clusterHealthCache);

            // Add managers
            services.AddScoped<IAreaManager, AreaManager>();
            services.AddSingleton<IBlobStorageManager, BlobStorageManager>();
            services.AddSingleton<IChecklistManager, ChecklistManager>();
            services.AddScoped<IDataProviderManager, DataProviderManager>();
            services.AddScoped<IDataQualityManager, DataQualityManager>();
            services.AddScoped<IDataStewardshipManager, DataStewardshipManager>();
            services.AddScoped<IDevOpsManager, DevOpsManager>();
            services.AddScoped<IExportManager, ExportManager>();
            services.AddScoped<IFilterManager, FilterManager>();
            services.AddScoped<ILocationManager, LocationManager>();
            services.AddScoped<IObservationManager, ObservationManager>();
            services.AddScoped<IProcessInfoManager, ProcessInfoManager>();
            services.AddScoped<IProjectManager, ProjectManager>();
            services.AddScoped<ITaxonListManager, TaxonListManager>();
            services.AddSingleton<ITaxonManager, TaxonManager>();
            services.AddScoped<ITaxonSearchManager, TaxonSearchManager>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IVocabularyManager, VocabularyManager>();
            services.AddSingleton<IApiUsageStatisticsManager, ApiUsageStatisticsManager>();

            // Add repositories
            services.AddScoped<IApiUsageStatisticsRepository, ApiUsageStatisticsRepository>();
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IDataProviderRepository, DataProviderRepository>();
            services.AddScoped<IDatasetRepository, DatasetRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IProcessedChecklistRepository, ProcessedChecklistRepository>();
            services.AddScoped<IUserObservationRepository, UserObservationRepository>();
            services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
            services.AddScoped<IProcessedLocationRepository, ProcessedLocationRepository>();
            services.AddScoped<IProcessedObservationRepository, ProcessedObservationRepository>();
            services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
            services.AddScoped<IProcessedTaxonRepository, ProcessedTaxonRepository>();
            services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
            services.AddScoped<IProtectedLogRepository, ProtectedLogRepository>();
            services.AddScoped<ITaxonRepository, TaxonRepository>();
            services.AddScoped<IVocabularyRepository, VocabularyRepository>();
            services.AddScoped<IProjectInfoRepository, ProjectInfoRepository>();
            services.AddScoped<ITaxonListRepository, TaxonListRepository>();
            services.AddScoped<IUserExportRepository, UserExportRepository>();

            // Add services
            services.AddSingleton<IBlobStorageService, BlobStorageService>();
            services.AddSingleton<ICryptoService, CryptoService>();
            services.AddSingleton<IDevOpsService, DevOpsService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IArtportalenApiService, ArtportalenApiService>();

            // Add Utilites
            services.AddSingleton<ISearchFilterUtility, SearchFilterUtility>();
            services.AddScoped<IGeneralizationResolver, GeneralizationResolver>();

            // Add Validators
            services.AddScoped<IInputValidator, InputValidator>();

            // Add writers
            services.AddScoped<IDwcArchiveFileWriter, DwcArchiveFileWriter>();
            services.AddScoped<IDwcArchiveFileWriterCoordinator, DwcArchiveFileWriterCoordinator>();
            services.AddScoped<IDwcArchiveEventCsvWriter, DwcArchiveEventCsvWriter>();
            services.AddScoped<IDwcArchiveEventFileWriter, DwcArchiveEventFileWriter>();
            services.AddScoped<IDwcArchiveOccurrenceCsvWriter, DwcArchiveOccurrenceCsvWriter>();
            services.AddScoped<IExtendedMeasurementOrFactCsvWriter, ExtendedMeasurementOrFactCsvWriter>();
            services.AddScoped<ISimpleMultimediaCsvWriter, SimpleMultimediaCsvWriter>();

            services.AddScoped<ICsvFileWriter, CsvFileWriter>();
            services.AddScoped<IExcelFileWriter, ExcelFileWriter>();
            services.AddScoped<IGeoJsonFileWriter, GeoJsonFileWriter>();

            // Helpers, static data => single instance 
            services.AddSingleton<IVocabularyValueResolver, VocabularyValueResolver>();
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="apiVersionDescriptionProvider"></param>
        /// <param name="configuration"></param>
        /// <param name="applicationInsightsConfiguration"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="protectedLogRepository"></param>
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider apiVersionDescriptionProvider,
            TelemetryConfiguration configuration,
            Lib.Configuration.Shared.ApplicationInsights applicationInsightsConfiguration,
            ObservationApiConfiguration observationApiConfiguration,
            IProtectedLogRepository protectedLogRepository)
        {
            if (observationApiConfiguration.EnableResponseCompression)
            {
                app.UseResponseCompression();
            }

            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
#if DEBUG
            configuration.DisableTelemetry = true;
#endif

            if (applicationInsightsConfiguration.EnableRequestBodyLogging)
            {
                app.UseMiddleware<EnableRequestBufferingMiddelware>();
                app.UseMiddleware<StoreRequestBodyMiddleware>();
            }

            if (!_disableHangfireInit)
            {
                app.UseHangfireDashboard();
            }

            // Allow client calls
            app.UseCors(cors => cors
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()            
            );

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{InternalApiName}{description.GroupName}/swagger.json",
                        $"SOS Observations API (Internal) {description.GroupName.ToUpperInvariant()}");

                    options.SwaggerEndpoint(
                        $"/swagger/{PublicApiName}{description.GroupName}/swagger.json",
                        $"SOS Observations API (Public) {description.GroupName.ToUpperInvariant()}");

                    options.SwaggerEndpoint(
                       $"/swagger/{AzureInternalApiName}{description.GroupName}/swagger.json",
                       $"SOS Observations API (Internal - Azure) {description.GroupName.ToUpperInvariant()}");

                    options.SwaggerEndpoint(
                        $"/swagger/{AzurePublicApiName}{description.GroupName}/swagger.json",
                        $"SOS Observations API (Public - Azure) {description.GroupName.ToUpperInvariant()}");

                    options.DisplayOperationId();
                    options.DocExpansion(DocExpansion.None);
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
#if !DEBUG
                if (!_disableHealthCheckInit)
                {
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = _ => false,
                        ResponseWriter = (context, _) => UIResponseWriter.WriteHealthCheckUIResponse(context, HealthChecks.Custom.HealthReportCachePublisher.LatestNoWfs)
                    });
                    endpoints.MapHealthChecks("/health-json", new HealthCheckOptions()
                    {
                        Predicate = _ => false,
                        ResponseWriter = async (context, _) =>
                        {
                            var report = HealthReportCachePublisher.LatestAll;
                            var result = report == null ? "{}" : Newtonsoft.Json.JsonConvert.SerializeObject(
                                new
                                {
                                    status = report.Status.ToString(),
                                    duration = report.TotalDuration,
                                    entries = report.Entries.Select(e => new
                                    {
                                        key = e.Key,
                                        description = e.Value.Description,
                                        duration = e.Value.Duration,
                                        status = Enum.GetName(typeof(HealthStatus),
                                            e.Value.Status),
                                        tags = e.Value.Tags
                                    }).ToList()
                                }, Newtonsoft.Json.Formatting.None,
                                new Newtonsoft.Json.JsonSerializerSettings
                                {
                                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                                });
                            context.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
                            await context.Response.WriteAsync(result);
                        }
                    });
                    endpoints.MapHealthChecks("/health-wfs", new HealthCheckOptions()
                    {
                        Predicate = _ => false,
                        ResponseWriter = (context, _) => UIResponseWriter.WriteHealthCheckUIResponse(context, HealthChecks.Custom.HealthReportCachePublisher.LatestOnlyWfs)
                    });
                }
#endif
            });

            // make sure protected log is created and indexed
            if (protectedLogRepository.VerifyCollectionAsync().Result)
            {
                protectedLogRepository.CreateIndexAsync();
            }

            var serviceProvider = app.ApplicationServices;
            var taxonSearchManager = serviceProvider.GetService<ITaxonSearchManager>();
            if (!_disableCachedTaxonSumAggregationInit)
            {
                Task.Run(() =>
                {
                    taxonSearchManager.GetCachedTaxonSumAggregationItemsAsync(new int[] { 0 });
                });
            }
        }

        private static IReadOnlyList<ApiVersion> GetApiVersions(ApiDescription apiDescription)
        {
            var apiVersionMetadata = apiDescription.ActionDescriptor.GetApiVersionMetadata();
            var actionApiVersionModel = apiVersionMetadata.Map(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

            var apiVersions = actionApiVersionModel.DeclaredApiVersions.Any()
                ? actionApiVersionModel.DeclaredApiVersions
                : actionApiVersionModel.ImplementedApiVersions;
            return apiVersions;
        }

        private static bool GetDisableFeature(string environmentVariable)
        {
            string value = Environment.GetEnvironmentVariable(environmentVariable);

            if (value != null)
            {
                if (bool.TryParse(value, out var disableHangfireInit))
                {
                    return disableHangfireInit;
                }

                return false;
            }
            else
            {
                return false;
            }
        }
    }
}