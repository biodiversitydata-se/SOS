using System;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NLog.Extensions.Logging;
using NLog.Web;
using SOS.Search.Service.Configuration;
using SOS.Search.Service.Factories;
using SOS.Search.Service.Factories.Interfaces;
using SOS.Search.Service.Repositories;
using SOS.Search.Service.Repositories.Interfaces;
using Swashbuckle.AspNetCore.Swagger;

namespace SOS.Search.Service
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration
        /// </summary>
		public static IConfiguration Configuration;

        private readonly bool _enableSwagger;

        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="env"></param>
        public Startup(IHostingEnvironment env)
        {
            env.EnvironmentName = "local";
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = configurationBuilder.Build();

            //Enable swagger for dev and local only
            _enableSwagger = new [] { "dev", "local" }.Contains(env.EnvironmentName);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Mvc Core services
            services.AddMvcCore(opts =>
                {
                    //opts.Filters.Add(new AllowAnonymousFilter());
                })
                .AddApiExplorer()
                .AddJsonFormatters();
                //.AddAuthorization();

            services.Configure<MongoDbConfiguration>(Configuration.GetSection("MongoDbConfiguration"));

            // Add CORS
            services.AddCors();

            // Add options
            services.AddOptions();

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(logger => logger.SetMinimumLevel(LogLevel.Trace));

            // Mongo Db
            var mongoConfiguration = Configuration.GetSection("MongoDbConfiguration").Get<MongoDbConfiguration>();

            var settings = new MongoClientSettings
            {
                Servers = mongoConfiguration.Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)),
                ReplicaSetName = mongoConfiguration.ReplicaSetName,
                UseTls = mongoConfiguration.UseTls,
                SslSettings = mongoConfiguration.UseTls ? new SslSettings
                {
                    EnabledSslProtocols = SslProtocols.Tls12
                } : null
            };

            if (!string.IsNullOrEmpty(mongoConfiguration.DatabaseName) &&
                !string.IsNullOrEmpty(mongoConfiguration.Password))
            {
                var identity = new MongoInternalIdentity(mongoConfiguration.DatabaseName, mongoConfiguration.UserName);
                var evidence = new PasswordEvidence(mongoConfiguration.Password);

                settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);
            }

            // Setup db
            services.AddSingleton<IMongoClient, MongoClient>(x => new MongoClient(settings));

            // Add factories
            services.AddScoped<ISightingFactory, SightingFactory>();
          
            // Repositories mongo
            services.AddScoped<ISightingAggregateRepository, SightingAggregateRepository>();
            
            // HttpContext
          //  services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
          //  services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

            // Configure swagger
            if (_enableSwagger)
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1",
                        new Info
                        {
                            Version = "v1",
                            Title = "DataService",
                            Description = "Asta data repository",
                            TermsOfService = "None"
                        });
                    options.IncludeXmlComments(GetXmlCommentsPath());
                    options.DescribeAllEnumsAsStrings();
                });
            }
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Add NLog support.
            loggerFactory.AddNLog();
            env.ConfigureNLog($"nlog.{env.EnvironmentName}.config");

            // Use CORS in the application
            app.UseCors(x => x.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            // app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc();

            // Use Swagger and SwaggerUI
            if (_enableSwagger)
            {
                app.UseSwagger(c => { c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value); });
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Indexing Service V1"); });
            }
        }

        /// <summary>
        /// Get xml file path
        /// </summary>
        /// <returns></returns>
        private static string GetXmlCommentsPath()
        {
            var assemblyName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            return Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.xml");
        }
    }
}
