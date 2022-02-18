using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SOS.Administration.Gui.Services;
using SOS.Lib.Configuration.Shared;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Nest;
using SOS.Administration.Gui.Managers;
using SOS.Administration.Gui.Managers.Interfaces;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Database;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Managers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Configuration;
using SOS.Lib.Repositories.Processed;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.ApplicationInsights;

namespace SOS.Administration.Gui
{
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
            _isDevelopment =  environment.Equals("local");
            if (_isDevelopment)
            {
                // If Development mode, add secrets stored on developer machine 
                // (%APPDATA%\Microsoft\UserSecrets\92cd2cdb-499c-480d-9f04-feaf7a68f89c\secrets.json)
                // In production you should store the secret values as environment variables.
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddMvcCore().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
         
            var authConfig = Configuration.GetSection(nameof(AuthenticationConfiguration)).Get<AuthenticationConfiguration>();
            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authConfig.Issuer,
                    ValidAudience = authConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.SecretKey))
                };
            });
            services.Configure<MongoDbConfiguration>(
                Configuration.GetSection(nameof(MongoDbConfiguration)));

            // Processed Mongo Db
            var processedDbConfiguration = Configuration.GetSection("ProcessDbConfiguration").Get<MongoDbConfiguration>();
            var processedSettings = processedDbConfiguration.GetMongoDbSettings();
            services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processedDbConfiguration.DatabaseName,
                processedDbConfiguration.ReadBatchSize, processedDbConfiguration.WriteBatchSize));

            //setup the elastic search configuration
            var elasticConfiguration = Configuration.GetSection("SearchDbConfiguration").Get<ElasticSearchConfiguration>();
            services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));
            var testElasticSearchConfiguration = Configuration.GetSection("SearchDbConfigurationTest").Get<TestElasticSearchConfiguration>();

            services.AddSingleton(authConfig);
            services.AddSingleton(elasticConfiguration);
            services.AddSingleton(testElasticSearchConfiguration);
            
            // Add cache
            services.AddSingleton<IClassCache<ProcessedConfiguration>, ClassCache<ProcessedConfiguration>>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();

            services.Configure<ApiTestConfiguration>(
              Configuration.GetSection(nameof(ApiTestConfiguration)));

            services.Configure<ApplicationInsightsConfiguration>(Configuration.GetSection(nameof(ApplicationInsightsConfiguration)));
            services.AddTransient<ISearchService, SearchService>();
            services.AddTransient<ITestService, TestService>();

            services.AddSingleton(Configuration.GetSection(nameof(ApplicationInsightsConfiguration)).Get<ApplicationInsightsConfiguration>());
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddScoped<IApplicationInsightsService, ApplicationInsightsService>();

            services.AddScoped<IProtectedLogManager, ProtectedLogManager>();

            services.AddScoped<IProtectedLogRepository, ProtectedLogRepository>();
            services.AddScoped<IProcessedObservationRepository, ProcessedObservationRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!_isDevelopment)
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (_isDevelopment)
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
