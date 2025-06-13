using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SOS.Administration.Gui.Managers;
using SOS.Administration.Gui.Managers.Interfaces;
using SOS.Administration.Gui.Services;
using SOS.Lib.Services;
using SOS.Lib.Services.Interfaces;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace SOS.Administration.Gui
{
    public class Startup
    {
        private bool _isDevelopment;

        public Startup(bool isDevelopment)
        {
            _isDevelopment = isDevelopment;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Use Swedish culture info.
            var culture = new CultureInfo("sv-SE");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            services.AddHealthChecks()
                .AddCheck<HealthCheck>("CustomHealthCheck", tags: ["k8s"]);            

            // Add CORS
            services.AddCors(o => o.AddPolicy("allowedOriginsPolicy", services =>
            {
                services.WithOrigins(Settings.AllowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader();

            }));            
            services.AddControllersWithViews();

            services.AddMvcCore().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            var authConfig = Settings.AuthenticationConfiguration;
            services.AddAuthentication(opt =>
            {
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

            // Processed Mongo Db
            var processDbConfiguration = Settings.ProcessDbConfiguration;
            var processedSettings = processDbConfiguration.GetMongoDbSettings();
            services.AddScoped<IProcessClient, ProcessClient>(p => new ProcessClient(processedSettings, processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize, processDbConfiguration.WriteBatchSize));

            //setup the elastic search configuration
            var elasticConfiguration = Settings.SearchDbConfiguration;
            services.AddSingleton<IElasticClientManager, ElasticClientManager>(p => new ElasticClientManager(elasticConfiguration));
            var testElasticSearchConfiguration = Settings.SearchDbTestConfiguration;

            services.AddSingleton(authConfig);
            services.AddSingleton(elasticConfiguration);
            services.AddSingleton(testElasticSearchConfiguration);

            // Add cache
            services.AddSingleton<IClassCache<ProcessedConfiguration>, ClassCache<ProcessedConfiguration>>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton(Settings.ApiTestConfiguration);

            services.AddTransient<ISearchService, SearchService>();
            services.AddTransient<ITestService, TestService>();

            services.AddSingleton(Settings.ApplicationInsightsConfiguration);
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddScoped<IApplicationInsightsService, ApplicationInsightsService>();

            services.AddScoped<IProtectedLogManager, ProtectedLogManager>();

            services.AddScoped<IProtectedLogRepository, ProtectedLogRepository>();
            services.AddScoped<IProcessedObservationCoreRepository, ProcessedObservationCoreRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(WebApplication app, IWebHostEnvironment env)
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

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Placeholder healthcheck for k8s deployment
            app.MapHealthChecks("/healthz", new HealthCheckOptions()
            {
                Predicate = r => r.Tags.Contains("k8s")
            });            

            // Use CORS
            app.UseCors("allowedOriginsPolicy");
            
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");
        }
    }
}
