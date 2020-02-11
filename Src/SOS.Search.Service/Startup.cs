using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Autofac;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using NLog.Web;
using SOS.Core.Repositories;
using SOS.Lib.Configuration.Shared;
using SOS.Search.Service.Factories;
using SOS.Search.Service.Factories.Interfaces;
using SOS.Search.Service.Repositories;
using SOS.Search.Service.Repositories.Interfaces;
using SOS.Search.Service.Swagger;

namespace SOS.Search.Service
{
    /// <summary>
    /// Program class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Auto fac
        /// </summary>
        public ILifetimeScope AutofacContainer { get; private set; }

        private string _environment;

        
        internal class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
        {
            /// <summary>
            /// 
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

        /// <summary>
        /// Start up
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            var _environment = env.EnvironmentName.ToLower();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_environment}.json", optional: true)
                .AddEnvironmentVariables();

            //Add secrets stored on developer machine (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
            if (new[] { "dev", "local" }.Contains(_environment))
            {
                builder.AddUserSecrets<Startup>();
            }
           
            Configuration = builder.Build();
        }
        public void ConfigureLogging(ContainerBuilder builder, ILoggingBuilder logging)
        {
            logging.ClearProviders();
            logging.AddConfiguration(Configuration.GetSection("Logging"))
                .AddNLog(configFileName: $"nlog.{_environment}.config");
        }

        /// <summary>
        /// Register Autofac services. This runs after ConfigureServices so the things
        /// here will override registrations made in ConfigureServices.
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            var mongoConfiguration = Configuration.GetSection("HangfireDbConfiguration").Get<MongoDbConfiguration>();

            var repositorySettings = new RepositorySettings
            {
                JobsDatabaseName = mongoConfiguration.DatabaseName,
                MongoDbConnectionString = $"mongodb://{string.Join(",", mongoConfiguration.Hosts.Select(h => h.Name))}"
            };

            builder.Register(r => repositorySettings).As<IRepositorySettings>().SingleInstance();
        }

        // <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Mvc Core services
            services.AddMvcCore(option => { option.EnableEndpointRouting = false; })
                .AddApiExplorer()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.Configure<MongoDbConfiguration>(Configuration.GetSection("ProcessedDbConfiguration"));

            // Add CORS
            services.AddCors();

            // Add options
            services.AddOptions();

            // Mongo Db
            var config = Configuration.GetSection("ProcessedDbConfiguration").Get<MongoDbConfiguration>();

            // Setup db
            services.AddSingleton<IMongoClient, MongoClient>(x => new MongoClient(config.GetMongoDbSettings()));

            // Add factories
            services.AddScoped<ISightingFactory, SightingFactory>();
            services.AddScoped<IProcessInfoFactory, ProcessInfoFactory>();
            services.AddSingleton<ITaxonFactory, TaxonFactory>();
            services.AddSingleton<IFieldMappingFactory, FieldMappingFactory>();

            // Repositories mongo
            services.AddScoped<IProcessedSightingRepository, ProcessedSightingRepository>();
            services.AddScoped<IProcessInfoRepository, ProcessInfoRepository>();
            services.AddScoped<IProcessedTaxonRepository, ProcessedTaxonRepository>();
            services.AddScoped<IProcessedFieldMappingRepository, ProcessedFieldMappingRepository>();

            // Configure swagger
            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1",
                        new OpenApiInfo
                        {
                            Title = "SOS.Search.Service",
                            Version = "v1",
                            Description = "Search sightings"

                        });
                            // Set the comments path for the Swagger JSON and UI.
                            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                    options.IncludeXmlComments(xmlPath);

                    options.SchemaFilter<SwaggerIgnoreFilter>();
                });

            // Hangfire
            var mongoConfiguration = Configuration.GetSection("HangfireDbConfiguration").Get<MongoDbConfiguration>();

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
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Use CORS in the application
            app.UseCors(x => x.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            if (new[] { "dev", "local" }.Contains(env.EnvironmentName.ToLower()))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            {
                Authorization = new[] { new AllowAllConnectionsFilter() },
                IgnoreAntiforgeryToken = true
            });

            // app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseStaticFiles();

            // Use Swagger and SwaggerUI
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SOS Search service");
            });
        }
    }
}
