using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SOS.Core;
using SOS.Core.IoC;
using SOS.Core.Jobs;
using SOS.Core.Repositories;
using Swashbuckle.AspNetCore.Swagger;

namespace SOS.Hangfire.UI
{
    public class Startup
    {
        IHostingEnvironment _environment;
        IConfigurationRoot _configuration;

        public Startup(Microsoft.AspNetCore.Hosting.IHostingEnvironment environment)
        {
            _environment = environment;
            SetConfiguration();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var container = BootstrapContainer.Boostrap();
            var repositorySettings = CreateRepositorySettings();
            SystemSettings.InitSettings(repositorySettings);
            container.Register(r => repositorySettings).As<IRepositorySettings>().SingleInstance();
            container.RegisterType<VerbatimTestDataHarvestJob>().As<IVerbatimTestDataHarvestJob>().InstancePerLifetimeScope();
            container.RegisterType<VerbatimTestDataProcessJob>().As<IVerbatimTestDataProcessJob>().InstancePerLifetimeScope();

            // Swagger
            services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1",
                        new Info
                        {
                            Title = "SOS.Hangfire.UI",
                            Version = "v1",
                            Description = "An API to handle various processing jobs",
                            TermsOfService = "None"
                        });
                    options.DescribeAllEnumsAsStrings();
                    // Set the comments path for the Swagger JSON and UI.
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    options.IncludeXmlComments(xmlPath);
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            

            // Hangfire
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMongoStorage(repositorySettings.MongoDbConnectionString, repositorySettings.JobsDatabaseName, MongoStorageOptions));

            // Add internal IoC
            container.Populate(services);

            IContainer autofacContainer = container.Build();
            GlobalConfiguration.Configuration.UseAutofacActivator(autofacContainer);

            return new AutofacServiceProvider(autofacContainer);
        }

        private IRepositorySettings CreateRepositorySettings()
        {
            var configuration = _configuration.GetSection("ApplicationSettings").GetSection("MongoDbRepository");

            return new RepositorySettings
            {
                JobsDatabaseName = configuration.GetValue<string>("JobsDatabaseName"),
                DatabaseName = configuration.GetValue<string>("DatabaseName"),
                MongoDbConnectionString = configuration.GetValue<string>("InstanceUrl")
            };
        }

        private void SetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(_environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{_environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            //Add secrets stored on developer machine (%APPDATA%\Microsoft\UserSecrets\1309b7a2-a1d5-40a3-b1dc-1a3aa53d09dc\secrets.json)
            if (_environment.IsDevelopment() ||
                _environment.EnvironmentName == "DEV" ||
                _environment.EnvironmentName == "LOCAL")
            {
                builder.AddUserSecrets<Startup>();
            }

            _configuration = builder.Build();
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            {
                Authorization = new[] { new AllowAllConnectionsFilter() },
                IgnoreAntiforgeryToken = true
            });

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ObservationProcessingJobs API, version 1");
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private static MongoStorageOptions MongoStorageOptions
        {
            get
            {
                var migrationOptions = new MongoMigrationOptions
                {
                    Strategy = MongoMigrationStrategy.Migrate,
                    BackupStrategy = MongoBackupStrategy.Collections
                };

                var storageOptions = new MongoStorageOptions { MigrationOptions = migrationOptions };
                return storageOptions;
            }
        }
    }

    public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // Allow outside. You need an authentication scenario for this part.
            // DON'T GO PRODUCTION WITH THIS LINES.
            return true;
        }
    }
}
