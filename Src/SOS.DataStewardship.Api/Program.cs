using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog.Formatting.Compact;
using SOS.DataStewardship.Api.Endpoints;
using SOS.DataStewardship.Api.Extensions;
using System.Text.Json.Serialization;

// human readable in the terminal when developing, not all json
var localDevConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

// compact json when running in the clusters for that sweet sweet structured logging
var inClusterConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var isDevelopment = new[] { "local", "dev", "st" }.Contains(env, StringComparer.CurrentCultureIgnoreCase);

// we set up Log.Logger here in order to be able to log if something goes wrong in the startup process
Log.Logger = isDevelopment ? localDevConfig : inClusterConfig;

try
{
    Log.Information("Starting up");
    var builder = WebApplication.CreateBuilder(args);

    // we register serilog with DI here so that we can use it in our application endpoints/handlers/services etc..
    builder.Logging.AddSerilog(isDevelopment ? localDevConfig : inClusterConfig);

    builder.Services.AddMemoryCache();
    builder.SetupUserSecrets();

    //we register our settings class with DI here so that we can use it in our application endpoints/handlers/services etc..
    // if some setting is missing or invalid we will throw an exception and the application will not start
    var settings = new Settings(builder.Configuration);
    builder.Services.AddSingleton<ISettings>(settings);

    builder.SetupAuthentication(settings);

    builder.SetupSwagger();    
    var processedDbConfiguration = builder.SetupDependencies(settings);
    builder.SetupHealthChecks(processedDbConfiguration);
    //builder.Services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true); // uncomment to debug bad requests
    builder.Services.AddEndpointDefinitions(typeof(IEndpointDefinition));
    builder.SetupJsonSerialization();
    builder.SetupMongoDbConventions();

    // This registration is needed to get Swagger enums to use strings instead of ints.
    builder.Services.Configure<JsonOptions>(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    
    builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

#if DEBUG
    builder.Services.Configure<TelemetryConfiguration>(x => x.DisableTelemetry = true);
#endif
    var app = builder.Build();

    app.ConfigureExceptionHandler(isDevelopment);
    app.UseEndpointDefinitions();    
    app.UseHealthChecks("/health", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    app.UseSwagger();        
    app.UseSwaggerUI(options =>
    {            
        options.DisplayOperationId();            
    });    

    app.UseAuthentication();
    //app.UseAuthorization();
    app.Run("http://*:5000");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }