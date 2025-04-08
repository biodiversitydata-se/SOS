using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using SOS.Analysis.Api.ApplicationInsights;
using SOS.Analysis.Api.Configuration;
using SOS.Analysis.Api.Middleware;
using SOS.Lib.ActionFilters;
using SOS.Lib.ApplicationInsights;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Middleware;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Globalization;
using SOS.Shared.Api.Utilities.Objects.Interfaces;
using SOS.Shared.Api.Utilities.Objects;
using SOS.Shared.Api.Validators.Interfaces;
using SOS.Shared.Api.Validators;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;
using Hangfire;
using Newtonsoft.Json.Converters;
using SOS.Analysis.Api.Repositories.Interfaces;
using SOS.Analysis.Api.Repositories;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using System.Security.Claims;
using SOS.Lib.Helpers;
using System.Collections.Concurrent;
using Serilog;
using Microsoft.IdentityModel.JsonWebTokens;
using Elastic.Clients.Elasticsearch.Cluster;
using SOS.Lib.JsonConverters;
using NetTopologySuite.Geometries;

namespace SOS.Analysis.Api
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Startup
    {
        private const string InternalApiName = "InternalSosAnalysis";
        private const string PublicApiName = "PublicSosAnalysis";
        private const string InternalApiPrefix = "Internal";
        private bool _disableHangfireInit = false;
        private bool _isDevelopment;
        private IWebHostEnvironment CurrentEnvironment { get; set; }

        private class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
        {
            readonly IApiVersionDescriptionProvider provider;

            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) =>
            this.provider = provider;

            public void Configure(SwaggerGenOptions options)
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, new OpenApiInfo
                    {
                        Version = description.ApiVersion.ToString(),
                        Title = $"API v{description.ApiVersion}",
                    });

                    options.SwaggerDoc(
                        $"{InternalApiName}{description.GroupName}",
                        new OpenApiInfo()
                        {
                            Title = $"SOS Analysis API (Internal) {description.GroupName.ToUpperInvariant()}",
                            Version = description.ApiVersion.ToString(),
                            Description = "Species Observation System (SOS) - Analysis API. Internal API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                        });

                    options.SwaggerDoc(
                        $"{PublicApiName}{description.GroupName}",
                        new OpenApiInfo()
                        {
                            Title = $"SOS Analysis API (Public) {description.GroupName.ToUpperInvariant()}",
                            Version = description.ApiVersion.ToString(),
                            Description = "Species Observation System (SOS) - Analysis API. Public API." + (description.IsDeprecated ? " This API version has been deprecated." : "")
                        });

                    options.CustomOperationIds(apiDesc =>
                    {
                        apiDesc.TryGetMethodInfo(out MethodInfo methodInfo);
                        var controller = apiDesc.ActionDescriptor?.RouteValues["controller"];
                        var methodName = methodInfo.Name;
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
            if (_isDevelopment)
            {
                // If Development mode, add secrets stored on developer machine 
                // (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
                // In production you should store the secret values as environment variables.
                builder.AddUserSecrets<Startup>();
            }
            _disableHangfireInit = GetDisableFeature(environmentVariable: "DISABLE_HANGFIRE_INIT");

            Configuration = builder.Build();
            Settings.Init(Configuration); // or fail early!
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
            services.AddMemoryCache();

            if (Settings.CorsAllowAny)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(name: "AllowAll", policy => policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin()
                    );
                });
            } 

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new GeometryConverter());
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddHealthChecks().AddCheck<HealthCheck>("CustomHealthCheck");

            // MongoDB conventions.
            ConventionRegistry.Register(
                "MongoDB Solution Conventions",
                new ConventionPack
                {
                    new IgnoreExtraElementsConvention(true),
                    new IgnoreIfNullConvention(true)
                },
                t => true);

           
            var userServiceConfiguration = Settings.UserServiceConfiguration;

            // Authentication
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = userServiceConfiguration.IdentityProvider.Audience;
                    options.Authority = userServiceConfiguration.IdentityProvider.Authority;
                    options.RequireHttpsMetadata = userServiceConfiguration.IdentityProvider.RequireHttpsMetadata;
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
                });

            // Add application insights.
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = Settings.ApplicationInsights.ConnectionString;
            });
            
            // Application insights custom
            services.AddApplicationInsightsTelemetryProcessor<IgnoreRequestPathsTelemetryProcessor>();
            services.AddSingleton(Settings.ApplicationInsights!);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
            
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

            services.AddSwaggerGen(options =>
                {
                    options.MapType<Geometry>(() => new OpenApiSchema { Type = "object" });

                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var xmlDocs = currentAssembly.GetReferencedAssemblies()
                        .Union(new AssemblyName[] { currentAssembly.GetName() })
                        .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly!.Location) ?? "", $"{a.Name}.xml"))
                        .Where(f => File.Exists(f)).ToArray();

                    Array.ForEach(xmlDocs, (d) =>
                    {
                        options.IncludeXmlComments(d);
                    });

                    options.UseInlineDefinitionsForEnums();

                    // add a custom operation filters
                    options.OperationFilter<SwaggerDefaultValues>();
                    options.OperationFilter<SwaggerAddOptionalHeaderParameters>();

                    options.SchemaFilter<SwaggerIgnoreFilter>();
                    // Post-modify Operation descriptions once they've been generated by wiring up one or more
                    // Operation filters.
                    options.OperationFilter<ApiManagementDocumentationFilter>();

                    options.DocInclusionPredicate((documentName, apiDescription) =>
                    {
                        var apiVersions = GetApiVersions(apiDescription);
                        bool isEndpointInternalApi = apiDescription.ActionDescriptor.EndpointMetadata.Any(x => x.GetType() == typeof(InternalApiAttribute));
                        if (isEndpointInternalApi && !documentName.StartsWith(InternalApiPrefix)) return false;
                        return apiVersions.Any(v =>
                                   $"{InternalApiName}v{v}" == documentName) ||
                               apiVersions.Any(v =>
                                   $"{PublicApiName}v{v}" == documentName);
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

            var analysisConfiguration = Settings.AnalysisConfiguration;

            // Response compression
            if (analysisConfiguration?.EnableResponseCompression ?? false)
            {
                services.AddResponseCompression(o => o.EnableForHttps = true);
                services.Configure<BrotliCompressionProviderOptions>(options =>
                {
                    options.Level = analysisConfiguration.ResponseCompressionLevel;
                });
                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = analysisConfiguration.ResponseCompressionLevel;
                });
            }

            //setup the elastic search configuration
            var elasticConfiguration = Settings.SearchDbConfiguration;
            services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));

            // Processed Mongo Db
            var processedDbConfiguration = Settings.ProcessDbConfiguration;
            if (processedDbConfiguration == null)
            {
                throw new Exception("Failed to get ProcessDbConfiguration");
            }

            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

            // Add configuration
            services.AddSingleton(analysisConfiguration!);
            services.AddSingleton(elasticConfiguration!);
            services.AddSingleton(Settings.InputValaidationConfiguration!);
            services.AddSingleton(Settings.UserServiceConfiguration!);
            services.AddSingleton(Settings.AreaConfiguration!);
            services.AddSingleton(Settings.CryptoConfiguration!);

            // Add security
            services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();

            // Add Caches
            services.AddSingleton<IAreaCache, AreaCache>();
            services.AddSingleton<IDataProviderCache, DataProviderCache>();
            services.AddSingleton<ICache<string, ProcessedConfiguration>, ProcessedConfigurationCache>();
            services.AddSingleton<IClassCache<TaxonListSetsById>, ClassCache<TaxonListSetsById>>();
            services.AddSingleton<IClassCache<TaxonTree<IBasicTaxon>>, ClassCache<TaxonTree<IBasicTaxon>>>();
            var clusterHealthCache = new ClassCache<ConcurrentDictionary<string, HealthResponse>>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<ConcurrentDictionary<string, HealthResponse>>>()) { CacheDuration = TimeSpan.FromMinutes(2) };
            services.AddSingleton<IClassCache<ConcurrentDictionary<string, HealthResponse>>>(clusterHealthCache);
            services.AddSingleton<SortableFieldsCache>();

            // Add managers            
            services.AddScoped<IAnalysisManager, AnalysisManager>();
            services.AddScoped<IFilterManager, FilterManager>();
            services.AddSingleton<ITaxonManager, TaxonManager>();

            // Add repositories
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IDataProviderRepository, DataProviderRepository>();
            services.AddScoped<IProcessedConfigurationRepository, ProcessedConfigurationRepository>();
            services.AddScoped<IProcessedObservationRepository, ProcessedObservationRepository>(); // Denna rad gör att det fungerar tillsammans med ProcessedObservationRepository
            services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();            
            services.AddScoped<ITaxonRepository, TaxonRepository>();
            services.AddScoped<ITaxonListRepository, TaxonListRepository>();
            services.AddScoped<IUserExportRepository, UserExportRepository>();

            // Add services
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddScoped<IUserService, UserService>();
            services.AddSingleton<ICryptoService, CryptoService>();
            services.AddSingleton<IFileService, FileService>();

            // Add Utilites
            services.AddSingleton<ISearchFilterUtility, SearchFilterUtility>();

            // Add Validators
            services.AddScoped<IInputValidator, InputValidator>();

            // Hangfire
            if (!_disableHangfireInit)
            {
                var mongoConfiguration = Settings.HangfireDbConfiguration;
                services.AddHangfire(configuration =>
                    configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings(m =>
                        {
                            m.Converters.Add(new NetTopologySuite.IO.Converters.GeometryConverter());
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
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="apiVersionDescriptionProvider"></param>
        /// <param name="telemetryConfiguration"></param>
        /// <param name="applicationInsightsConfiguration"></param>
        /// <param name="statisticsConfiguration"></param>
        public void Configure(
            IApplicationBuilder app,
            TelemetryConfiguration telemetryConfiguration,
            IApiVersionDescriptionProvider apiVersionDescriptionProvider,
            Lib.Configuration.Shared.ApplicationInsights applicationInsightsConfiguration,
            AnalysisConfiguration statisticsConfiguration)
        {
            if (Settings.CorsAllowAny)
            {
                app.UseCors("AllowAll");
            }

            if (!_disableHangfireInit)
            {
                app.UseHangfireDashboard();
            }
            
            if (statisticsConfiguration.EnableResponseCompression)
            {
                app.UseResponseCompression();
            }

            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
                telemetryConfiguration.DisableTelemetry = true;                
            }
            else
            {
                app.UseHsts();
            }

            app.UseMiddleware<LogApiUserTypeMiddleware>();
            if (applicationInsightsConfiguration.EnableRequestBodyLogging)
            {
                app.UseMiddleware<EnableRequestBufferingMiddelware>();
                app.UseMiddleware<StoreRequestBodyMiddleware>();
            }

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
                        $"SOS Analysis API (Internal) {description.GroupName.ToUpperInvariant()}");

                    options.SwaggerEndpoint(
                        $"/swagger/{PublicApiName}{description.GroupName}/swagger.json",
                        $"SOS Analysis API (Public) {description.GroupName.ToUpperInvariant()}");

                    options.DisplayOperationId();
                    options.DocExpansion(DocExpansion.None);
                }
            });

            app.UseHealthChecks("/healthz");
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Use Serilog request logging.
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    if (httpContext.Items.TryGetValue("UserId", out var userId))
                    {
                        diagnosticContext.Set("UserId", userId);
                    }

                    if (httpContext.Items.TryGetValue("Email", out var email))
                    {
                        diagnosticContext.Set("Email", email);
                    }

                    if (httpContext.Items.TryGetValue("Endpoint", out var endpoint))
                    {
                        diagnosticContext.Set("Endpoint", endpoint);
                    }

                    if (httpContext.Items.TryGetValue("QueryString", out var queryString))
                    {
                        diagnosticContext.Set("QueryString", queryString);
                    }

                    if (httpContext.Items.TryGetValue("Handler", out var handler))
                    {
                        diagnosticContext.Set("Handler", handler);
                    }  

                    try
                    {
                        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            string token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
                            var jsonWebTokenHandler = new JsonWebTokenHandler();
                            var jwt = jsonWebTokenHandler.ReadJsonWebToken(token);
                            if (jwt != null)
                            {
                                string? clientId = jwt.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
                                if (clientId != null) diagnosticContext.Set("ClientId", clientId);
                                string? name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                                if (name != null) diagnosticContext.Set("Name", name);
                                if (jwt.Subject != null) diagnosticContext.Set("Subject", jwt.Subject);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Error when deserializing JWT.");
                    }
                };
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
                if (bool.TryParse(value, out var val))
                {
                    return val;
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