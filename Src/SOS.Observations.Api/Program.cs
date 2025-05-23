using Hangfire;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;
using Serilog;
using SOS.Lib.Extensions;
using SOS.Lib.JsonConverters;
using SOS.Lib.Middleware;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Middleware;
using SOS.Shared.Api.Extensions.Dto;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// --- Program startup ---

try
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    bool isLocalDevelopment = new[] { "local", "k8s" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool isDevelopment = new[] { "local", "dev", "st" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool disableHangfireInit = Environment.GetEnvironmentVariable("DISABLE_HANGFIRE_INIT").GetBoolean();
    bool useLocalHangfire = Environment.GetEnvironmentVariable("USE_LOCAL_HANGFIRE").GetBoolean();
    bool disableHealthCheckInit = Environment.GetEnvironmentVariable("DISABLE_HEALTHCHECK_INIT").GetBoolean();
    bool disableCachedTaxonSumAggregationInit = Environment.GetEnvironmentVariable("DISABLE_CACHED_TAXON_SUM_INIT").GetBoolean();

    // Setup logging
    SerilogExtensions.SetupSerilog(isDevelopment);
    Log.Logger.Information("Starting Service");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(Log.Logger);

    // Set Swedish culture globally
    SetCulture("sv-SE");

    // Register MongoDB conventions
    RegisterMongoConventions();

    // Build configuration
    var configurationRoot = BuildConfiguration(builder, isDevelopment);
    Settings.Init(configurationRoot);

    // Register services
    ConfigureServices(builder.Services, configurationRoot, isDevelopment, disableHangfireInit, useLocalHangfire, disableHealthCheckInit, builder.Environment.IsEnvironment("prod"));

    // Build app and configure middleware pipeline
    var app = builder.Build();
    ConfigureMiddleware(app, isDevelopment, disableHangfireInit, disableHealthCheckInit);

    // Ensure protected log collection and index
    await EnsureProtectedLogIndexAsync(app);

    // Optionally initialize taxon sum aggregation cache
    if (!disableCachedTaxonSumAggregationInit)
        InitializeTaxonSumAggregationCache(app);

    // Start the application
    await app.RunAsync("http://*:5000");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// --- Helper methods for startup ---

static void SetCulture(string cultureName)
{
    var culture = new CultureInfo(cultureName);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}

static void RegisterMongoConventions()
{
    ConventionRegistry.Register(
        "MongoDB Solution Conventions",
        new ConventionPack { new IgnoreExtraElementsConvention(true), new IgnoreIfNullConvention(true) },
        t => true);
}

static IConfigurationRoot BuildConfiguration(WebApplicationBuilder builder, bool isDevelopment)
{
    var environment = builder.Environment.EnvironmentName.ToLower();
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{environment}.json", true)
        .AddEnvironmentVariables();
    if (isDevelopment)
        configBuilder.AddUserSecrets<Program>();
    return configBuilder.Build();
}

static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration, bool isDevelopment, bool disableHangfireInit, bool useLocalHangfire, bool disableHealthCheckInit, bool isProd)
{
    services.AddDependencyInjectionServices(configuration);

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

    services.AddMemoryCache();
    services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.AllowTrailingCommas = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new GeometryConverter());
            options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
        });
    services.SetupAuthentication();
    services.SetupSwagger();

    if (Settings.ObservationApiConfiguration.EnableResponseCompression)
    {
        services.AddResponseCompression(o => o.EnableForHttps = true);
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = Settings.ObservationApiConfiguration.ResponseCompressionLevel;
        });
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = Settings.ObservationApiConfiguration.ResponseCompressionLevel;
        });
    }
    if (!disableHangfireInit)
        services.SetupHangfire(useLocalHangfire);
    services.SetupHealthchecks(disableHealthCheckInit, isProd);
}

static void ConfigureMiddleware(WebApplication app, bool isDevelopment, bool disableHangfireInit, bool disableHealthCheckInit)
{
    if (Settings.CorsAllowAny)
        app.UseCors("AllowAll");
    if (Settings.ObservationApiConfiguration.EnableResponseCompression)
        app.UseResponseCompression();
    if (isDevelopment)
        app.UseDeveloperExceptionPage();
    else
        app.UseHsts();

    if (!app.Environment.IsEnvironment("prod"))
    {
        var telemetryConfig = app.Services.GetRequiredService<TelemetryConfiguration>();
        telemetryConfig.DisableTelemetry = true;
    }

    app.UseMiddleware<LogApiUserTypeMiddleware>();
    if (Settings.ApplicationInsightsConfiguration.EnableRequestBodyLogging)
    {
        app.UseMiddleware<EnableRequestBufferingMiddelware>();
        app.UseMiddleware<StoreRequestBodyMiddleware>();
    }
    if (!disableHangfireInit)
        app.UseHangfireDashboard();    
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.ApplyUseSerilogRequestLogging();
    app.MapControllers();
    app.ApplyMapHealthChecks(disableHealthCheckInit);
    app.UseSwagger();
    app.ApplyUseSwaggerUI();
}

static async Task EnsureProtectedLogIndexAsync(WebApplication app)
{
    var protectedLogRepository = app.Services.GetRequiredService<IProtectedLogRepository>();
    if (await protectedLogRepository.VerifyCollectionAsync())
        await protectedLogRepository.CreateIndexAsync();
}

static void InitializeTaxonSumAggregationCache(WebApplication app)
{
    var taxonSearchManager = app.Services.GetService<ITaxonSearchManager>();
    _ = Task.Run(async () =>
    {
        try
        {
            await taxonSearchManager.GetCachedTaxonSumAggregationItemsAsync(new int[] { 0 });
            Log.Logger.Information("TaxonSumAggregation cache initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed to initialize TaxonSumAggregation cache");
        }
    });
}

// Namespace declaration must come last for top-level statements
namespace SOS.Observations.Api
{
    /// <summary>
    /// Expose the implicitly defined Program class to the test project.
    /// This is needed for integration tests using WebApplicationFactory.
    /// </summary>
    public partial class Program { }
}