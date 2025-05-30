using Hangfire;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.ResponseCompression;
using MongoDB.Bson.Serialization.Conventions;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using SOS.Analysis.Api.Extensions;
using SOS.Analysis.Api.Middleware;
using SOS.Lib.Extensions;
using SOS.Lib.JsonConverters;
using SOS.Lib.Middleware;
using System.Globalization;
using System.Text.Json.Serialization;

// --- Program startup ---

try
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    bool isLocalDevelopment = new[] { "local", "k8s" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool isDevelopment = new[] { "local", "dev", "st" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool disableHangfireInit = Environment.GetEnvironmentVariable("DISABLE_HANGFIRE_INIT").GetBoolean();
    bool useLocalHangfire = Environment.GetEnvironmentVariable("USE_LOCAL_HANGFIRE").GetBoolean();

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
    ConfigureServices(builder.Services, configurationRoot, isDevelopment, disableHangfireInit, useLocalHangfire);

    // Build app and configure middleware pipeline
    var app = builder.Build();
    ConfigureMiddleware(app, isDevelopment, disableHangfireInit);

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

static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration, bool isDevelopment, bool disableHangfireInit, bool useLocalHangfire)
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
            options.JsonSerializerOptions.Converters.Add(new GeometryConverter());
            options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    services.SetupAuthentication();
    services.SetupSwagger();

    if (Settings.AnalysisConfiguration.EnableResponseCompression)
    {
        services.AddResponseCompression(o => o.EnableForHttps = true);
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = Settings.AnalysisConfiguration.ResponseCompressionLevel;
        });
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = Settings.AnalysisConfiguration.ResponseCompressionLevel;
        });
    }
    if (!disableHangfireInit)
        services.SetupHangfire(useLocalHangfire);
    services.SetupHealthchecks();
}

static void ConfigureMiddleware(WebApplication app, bool isDevelopment, bool disableHangfireInit)
{
    if (Settings.CorsAllowAny)
        app.UseCors("AllowAll");
    if (Settings.AnalysisConfiguration.EnableResponseCompression)
        app.UseResponseCompression();
    if (!disableHangfireInit)
        app.UseHangfireDashboard();
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
    if (Settings.ApplicationInsights.EnableRequestBodyLogging)
    {
        app.UseMiddleware<EnableRequestBufferingMiddelware>();
        app.UseMiddleware<StoreRequestBodyMiddleware>();
    }    
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.ApplyUseSerilogRequestLogging();    
    app.ApplyMapHealthChecks();
    app.UseSwagger();
    app.ApplyUseSwaggerUI();
    app.MapControllers();
}

// Namespace declaration for integration tests
namespace SOS.Analysis.Api
{
    /// <summary>
    /// Expose the implicitly defined Program class to the test project.
    /// This is needed for integration tests using WebApplicationFactory.
    /// </summary>
    public partial class Program { }
}