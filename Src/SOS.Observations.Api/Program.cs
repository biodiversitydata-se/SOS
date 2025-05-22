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
using Serilog.Filters;
using Serilog.Formatting.Compact;
using SOS.Lib.JsonConverters;
using SOS.Lib.Middleware;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Helpers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Middleware;
using SOS.Shared.Api.Extensions.Dto;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

try
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    bool isLocalDevelopment = new[] { "local", "k8s" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool _isDevelopment = new[] { "local", "dev", "st" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool _disableHangfireInit = StartupHelpers.GetEnvironmentBool(environmentVariable: "DISABLE_HANGFIRE_INIT");
    bool _useLocalHangfire = StartupHelpers.GetEnvironmentBool(environmentVariable: "USE_LOCAL_HANGFIRE");
    bool _disableHealthCheckInit = StartupHelpers.GetEnvironmentBool(environmentVariable: "DISABLE_HEALTHCHECK_INIT");
    bool _disableCachedTaxonSumAggregationInit = StartupHelpers.GetEnvironmentBool(environmentVariable: "DISABLE_CACHED_TAXON_SUM_INIT");

    // we set up Log.Logger here in order to be able to log if something goes wrong in the startup process
    Log.Logger = isLocalDevelopment ?
    new LoggerConfiguration() // human readable in the terminal when developing, not all json
        .MinimumLevel.Debug()
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties}{NewLine}{Exception}")
    .CreateLogger()
:
    new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console(new RenderedCompactJsonFormatter())
        .Enrich.FromLogContext()
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Http.Result", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Http.HttpResults", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Cors.Infrastructure", Serilog.Events.LogEventLevel.Warning)
        .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/healthz"))
    .CreateLogger();

    Log.Logger.Information("Starting Service");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog(Log.Logger);

    // Use Swedish culture info.
    var culture = new CultureInfo("sv-SE");
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    // MongoDB conventions.
    ConventionRegistry.Register("MongoDB Solution Conventions",
        new ConventionPack {new IgnoreExtraElementsConvention(true), new IgnoreIfNullConvention(true)},t => true);

    // Configuration
    var environment = builder.Environment.EnvironmentName.ToLower();
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{environment}.json", true)
        .AddEnvironmentVariables();
    if (_isDevelopment)
    {
        configBuilder.AddUserSecrets<Program>();
    }
    var configuration = configBuilder.Build();
    Settings.Init(configuration);

    // Service registration
    var services = builder.Services;
    builder.Services.AddDependencyInjectionServices(configuration);    
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
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never; // Some clients does not support omitting null values, so use JsonIgnoreCondition.Never for now.
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.Converters.Add(new GeometryConverter());
            options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()); // Used for FeatureCollections
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
    if (!_disableHangfireInit)
    {
        services.SetupHangfire(_useLocalHangfire);
    }
    services.SetupHealthchecks(_disableHealthCheckInit, builder.Environment.IsEnvironment("prod"));

    // Middleware and endpoint configuration
    var app = builder.Build();
    if (Settings.CorsAllowAny)    
        app.UseCors("AllowAll");    
    
    if (Settings.ObservationApiConfiguration.EnableResponseCompression)    
        app.UseResponseCompression();
    
    if (_isDevelopment)
        app.UseDeveloperExceptionPage();    
    else    
        app.UseHsts();    

    if (!builder.Environment.IsEnvironment("prod"))
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

    if (!_disableHangfireInit)    
        app.UseHangfireDashboard();
    
    app.UseSwagger();    
    app.ApplyUseSwaggerUI();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.ApplyUseSerilogRequestLogging();
    app.MapControllers();
    app.ApplyMapHealthChecks(_disableHealthCheckInit);    

    // make sure protected log is created and indexed
    var protectedLogRepository = app.Services.GetRequiredService<IProtectedLogRepository>();
    if (protectedLogRepository.VerifyCollectionAsync().Result)
    {
        await protectedLogRepository.CreateIndexAsync();
    }
    
    if (!_disableCachedTaxonSumAggregationInit)
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

    // Start the application and start listening for incoming requests.
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

namespace SOS.Observations.Api
{
    /// <summary>
    /// Expose the implicitly defined Program class to the test project.
    /// This is needed for integration tests using WebApplicationFactory.
    /// </summary>
    public partial class Program { }
}