using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Elasticsearch.Net;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nest;
using NLog.Web;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.JsonConverters;
using SOS.Observations.Api.Database;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Observations.Api.Swagger;
using SOS.Observations.Services;
using SOS.Observations.Services.Interfaces;

namespace SOS.Observations.Api
{
    /// <summary>
    ///     Program class
    /// </summary>
    public class Startup
    {
        private readonly string _environment;

        /// <summary>
        ///     Start up
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            _environment = env.EnvironmentName.ToLower();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{_environment}.json", true)
                .AddEnvironmentVariables();

            //Add secrets stored on developer machine (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
            builder.AddUserSecrets<Startup>();

            Configuration = builder.Build();
        }

        /// <summary>
        ///     Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     Auto fac
        /// </summary>
        public ILifetimeScope AutofacContainer { get; private set; }

        public void ConfigureLogging(ContainerBuilder builder, ILoggingBuilder logging)
        {
            logging.ClearProviders();
            logging.AddConfiguration(Configuration.GetSection("Logging"))
                .AddNLog($"nlog.{_environment}.config");
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
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            

            services.AddControllers()
                .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new GeoShapeConverter()); });

            // Identity service configuration
            var identityServerConfiguration = Configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();

            // Authentication
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = identityServerConfiguration.Authority;
                    options.RequireHttpsMetadata = identityServerConfiguration.RequireHttpsMetadata;
                    options.Audience = "SOS.Observations.Api";
                });

            // Add Mvc Core services
            services.AddMvcCore(option => { option.EnableEndpointRouting = false; })
                .AddApiExplorer()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // Configure swagger
            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1",
                        new OpenApiInfo
                        {
                            Title = "SOS.Observations.Api",
                            Version = "v1",
                            Description = "Search sightings"
                        });
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
                });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var observationApiConfiguration = Configuration.GetSection("ObservationApiConfiguration")
                .Get<ObservationApiConfiguration>();

            // Hangfire
            var mongoConfiguration = observationApiConfiguration.HangfireDbConfiguration;

            services.AddHangfire(configuration =>
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseMongoStorage(mongoConfiguration.GetMongoDbSettings(),
                        mongoConfiguration.DatabaseName,
                        new MongoStorageOptions
                        {
                            MigrationOptions = new MongoMigrationOptions
                            {
                                Strategy = MongoMigrationStrategy.Migrate,
                                BackupStrategy = MongoBackupStrategy.Collections
                            }
                        })
            );

            //setup the elastic search configuration
            var elasticConfiguration = observationApiConfiguration.SearchDbConfiguration;
            var uris = elasticConfiguration.Hosts.Select(u => new Uri(u));
            services.AddSingleton<IElasticClient>(
                new ElasticClient(new ConnectionSettings(new StaticConnectionPool(uris))));

            // Processed Mongo Db
            var processedDbConfiguration = observationApiConfiguration.ProcessedDbConfiguration;
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            var processClient = new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.BatchSize);
            services.AddSingleton<IProcessClient>(processClient);

            // Add configuration
            services.AddSingleton(observationApiConfiguration.BlobStorageConfiguration);
            services.AddSingleton(elasticConfiguration);

            // Add managers
            services.AddSingleton<IAreaManager, AreaManager>();
            services.AddSingleton<IDataProviderManager, DataProviderManager>();
            services.AddSingleton<IDOIManager, DOIManager>();
            services.AddSingleton<IFieldMappingManager, FieldMappingManager>();
            services.AddSingleton<IObservationManager, ObservationManager>();
            services.AddSingleton<IProcessInfoManager, ProcessInfoManager>();
            services.AddSingleton<ITaxonManager, TaxonManager>();

            // Add repositories
            services.AddSingleton<IAreaRepository, AreaRepository>();
            services.AddSingleton<IDataProviderRepository, DataProviderRepository>();
            services.AddSingleton<IDOIRepository, DOIRepository>();
            services.AddSingleton<IProcessedObservationRepository, ProcessedObservationRepository>();
            services.AddSingleton<IProcessInfoRepository, ProcessInfoRepository>();
            services.AddSingleton<IProcessedTaxonRepository, ProcessedTaxonRepository>();
            services.AddSingleton<IProcessedFieldMappingRepository, ProcessedFieldMappingRepository>();

            // Add services
            services.AddSingleton<IBlobStorageService, BlobStorageService>();
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            if (new[] {"dev", "local"}.Contains(env.EnvironmentName.ToLower()))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "SOS Search service"); });

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] {new AllowAllConnectionsFilter()},
                IgnoreAntiforgeryToken = true
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

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