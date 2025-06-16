using HealthChecks.UI.Client;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Bson.Serialization.Conventions;
using Serilog;
using SOS.ElasticSearch.Proxy.Extensions;
using SOS.ElasticSearch.Proxy.Middleware;
using System.Globalization;
using System.Text.Json.Serialization;

// --- Program startup ---

try
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    bool isLocalDevelopment = new[] { "local", "k8s" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool isDevelopment = new[] { "local", "dev", "st" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);    

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
    ConfigureServices(builder.Services, configurationRoot, isDevelopment);

    // Build app and configure middleware pipeline
    var app = builder.Build();
    ConfigureMiddleware(app, isDevelopment);

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
#if DEBUG
    configBuilder.AddUserSecrets<Program>();
#else
    if (isDevelopment)
        configBuilder.AddUserSecrets<Program>();
#endif
    return configBuilder.Build();
}

static void ConfigureServices(IServiceCollection services, IConfigurationRoot configuration, bool isDevelopment)
{
    services.AddDependencyInjectionServices(configuration);
    services.AddMvcCore(option => { option.EnableEndpointRouting = false; })
        .AddApiExplorer()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    services.AddMvc();
    services.SetupHealthchecks();
}

static void ConfigureMiddleware(WebApplication app, bool isDevelopment)
{    
    if (isDevelopment)
        app.UseDeveloperExceptionPage();
    else
        app.UseHsts();

    if (!app.Environment.IsEnvironment("prod"))
    {
        var telemetryConfig = app.Services.GetRequiredService<TelemetryConfiguration>();
        telemetryConfig.DisableTelemetry = true;
    }

    app.UseHealthChecks("/healthz");
    app.UseHealthChecks("/health", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });


    app.UseWhen(context => context.Request.Path.StartsWithSegments("/caches"),
            builder => builder
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            }
        )
    );

    app.UseMiddleware<RequestMiddleware>();
    app.ApplyUseSerilogRequestLogging();       
}