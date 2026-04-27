using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SOS.DataStewardship.Api.Endpoints;
using SOS.DataStewardship.Api.Extensions;
using SOS.Lib.Helpers;
using System.Text.Json.Serialization;



var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var isDevelopment = new[] { "local", "dev", "st" }.Contains(env, StringComparer.CurrentCultureIgnoreCase);

try
{
    // we set up Log.Logger here in order to be able to log if something goes wrong in the startup process
    Log.Logger = SeriLogHelper.CreateLogger(isDevelopment);
    Log.Information("Starting up");
    var builder = WebApplication.CreateBuilder(args);
    SeriLogHelper.ConfigureSerilog(builder, "datastewardship.api", isDevelopment);
    
    builder.AddServiceDefaults();
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
        options.JsonSerializerOptions.Converters.Add(new SOS.Lib.JsonConverters.GeometryConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    
    builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

#if DEBUG
    builder.Services.Configure<TelemetryConfiguration>(x => x.DisableTelemetry = true);
#endif
    var app = builder.Build();
    app.ApplyUseSerilogRequestLogging();
    app.MapDefaultEndpoints();
    app.ConfigureExceptionHandler(isDevelopment);
    app.UseEndpointDefinitions();


    app.MapHealthChecks("/healthz", new HealthCheckOptions()
    {
        Predicate = _ => false,        
    });
    app.UseHealthChecks("/health", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    //app.PreventSwaggerCaching();
    app.UseSwagger();        
    app.UseSwaggerUI(options =>
    {            
        options.DisplayOperationId();            
    });    

    app.UseAuthentication();
    //app.UseAuthorization();

    // Use Serilog request logging.
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
                       
        };
    });

    string? aspnetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    await app.RunAsync(aspnetCoreUrls ?? "http://*:5005");    
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