using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog.Web;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.ObservationApi;
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
using SOS.Observations.Api.ActionFilters;
using SOS.Observations.Api.HealthChecks;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Middleware;
using SOS.Observations.Api.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
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
        private const string InternalApiPrefix = "Internal";
        private IWebHostEnvironment CurrentEnvironment { get; set; }
        private bool _isDevelopment;
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

            _isDevelopment = environment.Equals("local");
            if (_isDevelopment)
            {
                // If Development mode, add secrets stored on developer machine 
                // (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
                // In production you should store the secret values as environment variables.
                builder.AddUserSecrets<Startup>();
            }

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
            services.AddMemoryCache();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new GeoShapeConverter());
                    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

            // Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = identityServerConfiguration.Audience;
                    options.Authority = identityServerConfiguration.Authority;
                    options.RequireHttpsMetadata = identityServerConfiguration.RequireHttpsMetadata;
                    options.TokenValidationParameters.RoleClaimType = "rname";
                });

            // Add application insights.
            services.AddApplicationInsightsTelemetry(Configuration);
            // Application insights custom
            services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
            services.AddSingleton(Configuration.GetSection("ApplicationInsights").Get<ApplicationInsights>());
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

            services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 5);
                o.ReportApiVersions = true;
                o.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
            });

            services.AddVersionedApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            var apiVersionDescriptionProvider =
                services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();
            services.AddSwaggerGen(
                swagger =>
                {
                    swagger.UseInlineDefinitionsForEnums();
                    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                    {                        
                        swagger.SwaggerDoc(
                            $"{InternalApiName}{description.GroupName}",
                            new OpenApiInfo()
                            {
                                Title = $"SOS Observations API (Internal) {description.GroupName.ToUpperInvariant()}",
                                Version = description.ApiVersion.ToString(),
                                Description = "Species Observation System (SOS) - Observations API. Internal API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                            });

                        swagger.SwaggerDoc(
                            $"{PublicApiName}{description.GroupName}",
                            new OpenApiInfo()
                            {
                                Title = $"SOS Observations API (Public) {description.GroupName.ToUpperInvariant()}",
                                Version = description.ApiVersion.ToString(),
                                Description = "Species Observation System (SOS) - Observations API. Public API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                            });

                        swagger.CustomOperationIds(apiDesc =>
                        {
                            apiDesc.TryGetMethodInfo(out MethodInfo methodInfo);
                            string controller = apiDesc.ActionDescriptor.RouteValues["controller"];
                            string methodName = methodInfo.Name;
                            return $"{controller}_{methodName}".Replace("Async", "", StringComparison.InvariantCultureIgnoreCase);
                        });
                    }

                    // add a custom operation filters
                    swagger.OperationFilter<SwaggerDefaultValues>();
                    swagger.OperationFilter<SwaggerAddOptionalHeaderParameters>();

                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var xmlDocs = currentAssembly.GetReferencedAssemblies()
                        .Union(new AssemblyName[] { currentAssembly.GetName() })
                        .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                        .Where(f => File.Exists(f)).ToArray();

                    Array.ForEach(xmlDocs, (d) =>
                    {
                        swagger.IncludeXmlComments(d);
                    });

                    swagger.SchemaFilter<SwaggerIgnoreFilter>();
                    // Post-modify Operation descriptions once they've been generated by wiring up one or more
                    // Operation filters.
                    swagger.OperationFilter<ApiManagementDocumentationFilter>();

                    swagger.DocInclusionPredicate((documentName, apiDescription) =>
                    {
                        var apiVersions = GetApiVersions(apiDescription);
                        bool isEndpointInternalApi = apiDescription.ActionDescriptor.EndpointMetadata.Any(x => x.GetType() == typeof(InternalApiAttribute));
                        if (isEndpointInternalApi && !documentName.StartsWith(InternalApiPrefix)) return false;
                        return apiVersions.Any(v =>
                                   $"{InternalApiName}v{v}" == documentName) ||
                               apiVersions.Any(v =>
                                   $"{PublicApiName}v{v}" == documentName);
                    });

                    swagger.AddSecurityDefinition("Bearer", //Name the security scheme
                        new OpenApiSecurityScheme
                        {
                            Name = "Authorization",
                            Description = "JWT Authorization header using the Bearer scheme.",
                            In = ParameterLocation.Header,
                            Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
                            Scheme = "bearer" //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                        });

                    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement{
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
            var mongoConfiguration = Configuration.GetSection("HangfireDbConfiguration").Get<HangfireDbConfiguration>();

            services.AddHangfire(configuration =>
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
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
            services.AddSingleton(elasticConfiguration);
            services.AddSingleton(Configuration.GetSection("UserServiceConfiguration").Get<UserServiceConfiguration>());
            services.AddSingleton(healthCheckConfiguration);
            services.AddSingleton(Configuration.GetSection("VocabularyConfiguration").Get<VocabularyConfiguration>());

            var healthChecks = services.AddHealthChecks()
                .AddDiskStorageHealthCheck(
                    x => x.AddDrive("C:\\", (long)(healthCheckConfiguration.MinimumLocalDiskStorage * 1000)),
                    name: $"Primary disk: min {healthCheckConfiguration.MinimumLocalDiskStorage}GB free - warning",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "disk" })
                .AddMongoDb(processedDbConfiguration.GetConnectionString(), tags: new [] { "database", "mongodb" })
                .AddHangfire(a => a.MinimumAvailableServers = 1, "Hangfire", tags: new[] { "hangfire" })
                .AddCheck<DataAmountHealthCheck>("Data amount", tags: new[] { "database", "elasticsearch", "data" })
                .AddCheck<SearchDataProvidersHealthCheck>("Search data providers", tags: new[] { "database", "elasticsearch", "query" })
                .AddCheck<SearchPerformanceHealthCheck>("Search performance", tags: new[] { "database", "elasticsearch", "query", "performance" })
                .AddCheck<AzureSearchHealthCheck>("Azure search API health check", tags: new[] { "azure", "database", "elasticsearch", "query" })
                .AddCheck<DataProviderHealthCheck>("Data providers", tags: new[] { "data providers", "meta data" })
                .AddCheck<WFSHealthCheck>("WFS", tags: new [] { "wfs" })
                .AddCheck<ElasticsearchProxyHealthCheck>("ElasticSearch Proxy", tags: new[] { "wfs", "elasticsearch" })
                .AddCheck<DuplicateHealthCheck>("Duplicate observations", tags: new[] { "elasticsearch", "harvest" });

            if (CurrentEnvironment.IsEnvironment("prod"))
            {
                healthChecks.AddCheck<DwcaHealthCheck>("DwC-A files", tags: new[] {"dwca", "export"});
                healthChecks.AddCheck<ApplicationInsightstHealthCheck>("Application Insights", tags: new[] {"application insights", "harvest"});
            }

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
            var taxonSumAggregationCache = new ClassCache<Dictionary<int, TaxonSumAggregationItem>>(new MemoryCache(new MemoryCacheOptions())) { CacheDuration = TimeSpan.FromHours(12) };
            services.AddSingleton<IClassCache<Dictionary<int, TaxonSumAggregationItem>>>(taxonSumAggregationCache);
            services.AddSingleton<IClassCache<HealthCheckResult>, ClassCache<HealthCheckResult>>();

            // Add managers
            services.AddScoped<IAreaManager, AreaManager>();
            services.AddSingleton<IBlobStorageManager, BlobStorageManager>();
            services.AddSingleton<IChecklistManager, ChecklistManager>();
            services.AddScoped<IDataProviderManager, DataProviderManager>();
            services.AddScoped<IDataQualityManager, DataQualityManager>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IExportManager, ExportManager>();
            services.AddScoped<IFilterManager, FilterManager>();
            services.AddScoped<ILocationManager, LocationManager>();
            services.AddScoped<IUserStatisticsManager, UserStatisticsManager>();
            services.AddScoped<IObservationManager, ObservationManager>();
            services.AddScoped<IProcessInfoManager, ProcessInfoManager>();
            services.AddScoped<IProjectManager, ProjectManager>();
            services.AddScoped<ITaxonListManager, TaxonListManager>();
            services.AddSingleton<ITaxonManager, TaxonManager>();
            services.AddScoped<ITaxonSearchManager, TaxonSearchManager>();
            services.AddScoped<IVocabularyManager, VocabularyManager>();
            services.AddScoped<IArtportalenApiManager, ArtportalenApiManager>();

            // Add repositories
            services.AddScoped<IApiUsageStatisticsRepository, ApiUsageStatisticsRepository>();
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IDataProviderRepository, DataProviderRepository>();
            services.AddScoped<IProcessedChecklistRepository, ProcessedChecklistRepository>();
            services.AddScoped<IUserObservationRepository, UserObservationRepository>();
            services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
            services.AddScoped<IProcessedLocationRepository, ProcessedLocationRepository>();
            services.AddScoped<IProcessedObservationRepository, ProcessedObservationRepository>();
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
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IArtportalenApiService, ArtportalenApiService>();

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
            ApplicationInsights applicationInsightsConfiguration, 
            ObservationApiConfiguration observationApiConfiguration,
            IProtectedLogRepository protectedLogRepository)
        {
            if (observationApiConfiguration.EnableResponseCompression)
            {
                app.UseResponseCompression();
            }
            NLogBuilder.ConfigureNLog($"nlog.{env.EnvironmentName}.config");
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

            app.UseHangfireDashboard();

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
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapHealthChecks("/health-json", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = async (context, report) =>
                    {
                        var result = JsonConvert.SerializeObject(
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
                            }, Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(result);
                    }
                });
            });

            // make sure protected log is created and indexed
            if (protectedLogRepository.VerifyCollectionAsync().Result)
            {
                protectedLogRepository.CreateIndexAsync();
            }

            var serviceProvider = app.ApplicationServices;
            var taxonSearchManager = serviceProvider.GetService<ITaxonSearchManager>();
            Task.Run(() => {
                taxonSearchManager.GetCachedTaxonSumAggregationItemsAsync(new int[] { 0 });                
            });

            
        }
        
        private static IReadOnlyList<ApiVersion> GetApiVersions(ApiDescription apiDescription)
        {
            var actionApiVersionModel = apiDescription.ActionDescriptor
                .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);

            var apiVersions = actionApiVersionModel.DeclaredApiVersions.Any()
                ? actionApiVersionModel.DeclaredApiVersions
                : actionApiVersionModel.ImplementedApiVersions;
            return apiVersions;
        }
    }
}