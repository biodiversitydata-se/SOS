using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using Serilog;
using SOS.Administration.Api.Extensions;
using System;
using System.Globalization;
using System.Linq;
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
    await app.RunAsync("http://*:5005");
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
    services.AddDependencyInjectionServices();
    services.AddMemoryCache();
    services.AddControllers()
        .AddJsonOptions(x => { x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });        
    services.SetupSwagger();
    if (!disableHangfireInit)
        services.SetupHangfire(useLocalHangfire);
    services.SetupHealthchecks();
}

static void ConfigureMiddleware(WebApplication app, bool isDevelopment, bool disableHangfireInit)
{
    if (!disableHangfireInit)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new AllowAllConnectionsFilter()],
            IgnoreAntiforgeryToken = true
        });
    }
    if (isDevelopment)
        app.UseDeveloperExceptionPage();
    else
        app.UseHsts();
    
    app.UseRouting();  
    app.UseAuthorization();
    app.ApplyUseSerilogRequestLogging();    
    app.ApplyMapHealthChecks();    
    app.ApplyUseSwagger();
    app.MapControllers();
}

public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}