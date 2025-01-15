using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json.Converters;
using SOS.Lib.JsonConverters;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Security.Interfaces;
using SOS.Lib.Security;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Processed;
using Serilog;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Http;

namespace SOS.Administration.Api
{
    /// <summary>
    ///     Start up class
    /// </summary>
    public class Startup
    {
        private bool _isDevelopment;

        /// <summary>
        ///     Start up
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            var environment = env.EnvironmentName.ToLower();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables();

            _isDevelopment = environment.Equals("local") || environment.Equals("dev") || environment.Equals("st");
            //Add secrets stored on developer machine (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
            if (_isDevelopment)
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
            Settings.Init(Configuration); // or fail early!
        }

        /// <summary>
        ///     Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     Auto fac
        /// </summary>
        public ILifetimeScope AutofacContainer { get; private set; }

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

            services.AddControllers()
                .AddJsonOptions(x => { x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue; // <-- !!! long.MaxValue
                options.MultipartBoundaryLengthLimit = int.MaxValue;
                options.MultipartHeadersCountLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
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

            // Swagger
            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1",
                        new OpenApiInfo
                        {
                            Title = "SOS.Administration.Api",
                            Version = "v1",
                            Description = "An API to handle various processing jobs" //,
                            //TermsOfService = "None"
                        });
                    // Set the comments path for the Swagger JSON and UI.
                    var currentAssembly = Assembly.GetExecutingAssembly();
                    var xmlDocs = currentAssembly.GetReferencedAssemblies()
                        .Union(new AssemblyName[] { currentAssembly.GetName() })
                        .Select(a => Path.Combine(Path.GetDirectoryName(currentAssembly.Location), $"{a.Name}.xml"))
                        .Where(f => File.Exists(f)).ToArray();

                    Array.ForEach(xmlDocs, (d) =>
                    {
                        options.IncludeXmlComments(d);
                    });
                });

            services.AddMvc();//.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddHealthChecks().AddCheck<HealthCheck>("CustomHealthCheck");

            // Hangfire
            var hangfireDbConfiguration = Settings.HangfireDbConfiguration;

            services.AddHangfire(configuration =>
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings(m =>
                    {
                        m.Converters.Add(new NewtonsoftGeoShapeConverter());
                        m.Converters.Add(new StringEnumConverter());
                    })
                    .UseMongoStorage(new MongoClient(hangfireDbConfiguration.GetMongoDbSettings()),
                        hangfireDbConfiguration.DatabaseName,
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

            var sosApiConfiguration = Settings.SosApiConfiguration;
            services.AddSingleton(sosApiConfiguration);
            var importConfiguration = Settings.ImportConfiguration;
            services.AddSingleton(importConfiguration.GeoRegionApiConfiguration);

            services.AddScoped<ICacheManager, CacheManager>();
            services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IApiUsageStatisticsManager, ApiUsageStatisticsManager>();
            services.AddSingleton<IApplicationInsightsService, ApplicationInsightsService>();
            ApiManagementServiceConfiguration apiMgmtServiceConfiguration = new ApiManagementServiceConfiguration();
            services.AddSingleton(apiMgmtServiceConfiguration);
            services.AddScoped<IAuthorizationProvider, CurrentUserAuthorization>();

            /*       // Add managers
                   services.AddSingleton<IIptManager, IIptManager>();

                   // Add services
                   services.AddSingleton<IFileDownloadService, FileDownloadService>();
                   services.AddSingleton<IHttpClientService, HttpClientService>();*/
        }

        /// <summary>
        ///     Register Autofac services. This runs after ConfigureServices so the things
        ///     here will override registrations made in ConfigureServices.
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {            
            AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Placeholder healthcheck for k8s deployment
            app.UseHealthChecks("/healthz");

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AllowAllConnectionsFilter() },
                IgnoreAntiforgeryToken = true
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ObservationProcessingJobs API, version 1");
            });
            
            app.UseRouting();

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

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }

    /// <summary>
    /// </summary>
    public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize(DashboardContext context)
        {
            // Allow outside. You need an authentication scenario for this part.
            // DON'T GO PRODUCTION WITH THIS LINES.
            return true;
        }
    }
}