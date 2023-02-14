using Microsoft.ApplicationInsights.Extensibility;
using SOS.DataStewardship.Api.Extensions;
using SOS.Lib.JsonConverters;
using System.Text.Json.Serialization;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var isDevelopment = new[] { "local", "dev" }.Contains(env, StringComparer.CurrentCultureIgnoreCase);
var logger = NLogBuilder.ConfigureNLog($"nlog.{env}.config").GetCurrentClassLogger();
logger.Info("Starting application...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddMemoryCache();
    
    builder.SetupUserSecrets();
    builder.SetupAuthentication();
    builder.SetupLogging();
    builder.SetupSwagger();
    builder.SetupDependencies();
    builder.RegisterModules();    
    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    {
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.SerializerOptions.Converters.Add(new GeoShapeConverter());
        options.SerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
    });

    // This registration is needed to get Swagger enums to use strings instead of ints.
    builder.Services.Configure<JsonOptions>(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

#if DEBUG
    builder.Services.Configure<TelemetryConfiguration>(x => x.DisableTelemetry = true);
#endif
    var app = builder.Build();
    app.ConfigureExceptionHandler(logger, isDevelopment);
    app.MapEndpoints();
    app.UseHttpsRedirection();
    
    app.UseSwagger();        
    app.UseSwaggerUI(options =>
    {            
        options.DisplayOperationId();            
    });    

    app.UseAuthentication();
    //app.UseAuthorization();
    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Failed to start application...");
    throw;
}
finally
{
    LogManager.Shutdown();
}

public partial class Program { }