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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nest;
using NLog.Web;
using SOS.Lib.Configuration.Shared;
using SOS.Observations.Api.Swagger;

namespace SOS.Observations.Api
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

        private readonly string _environment;

        /// <summary>
        /// Start up
        /// </summary>
        /// <param name="env"></param>
        public Startup(IWebHostEnvironment env)
        {
            _environment = env.EnvironmentName.ToLower();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_environment}.json", optional: true)
                .AddEnvironmentVariables();

            //Add secrets stored on developer machine (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
            if (env.IsDevelopment() ||
                _environment == "dev" ||
                _environment == "local")
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
            
        }

        /// <summary>
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
                            // Set the comments path for the Swagger JSON and UI.
                            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                    options.IncludeXmlComments(xmlPath);

                    options.SchemaFilter<SwaggerIgnoreFilter>();
                });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

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


            //setup the elastic search configuration
            var elasticConfiguration = Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();

            var uris = elasticConfiguration.Hosts.Select(u => new Uri(u));
            var nodes = uris.Select(u => new Node(u) { MasterEligible = true });

            var connectionPool = new SniffingConnectionPool(nodes);
            var settings = new ConnectionSettings(connectionPool)
                .SniffLifeSpan(TimeSpan.FromMinutes(1))
                .RequestTimeout(TimeSpan.FromMinutes(5));
            
            services.AddSingleton<IElasticClient>(new ElasticClient(settings));
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            if (new[] { "dev", "local" }.Contains(env.EnvironmentName.ToLower()))
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
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SOS Search service");
            });

            app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            {
                Authorization = new[] { new AllowAllConnectionsFilter() },
                IgnoreAntiforgeryToken = true
            });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
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

}