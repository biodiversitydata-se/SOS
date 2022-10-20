using Autofac.Core;
using Microsoft.ApplicationInsights.Extensibility;
using SOS.DataStewardship.Api.Extensions;
using SOS.Lib.JsonConverters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.Json;
using System.Text.Json.Serialization;
using static CSharpFunctionalExtensions.Result;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
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

#if DEBUG
    builder.Services.Configure<TelemetryConfiguration>(x => x.DisableTelemetry = true);
#endif
    var app = builder.Build();
    app.ConfigureExceptionHandler(logger, app.Environment.IsDevelopment());
    app.MapEndpoints();
    app.UseHttpsRedirection();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();        
        app.UseSwaggerUI(options =>
        {            
            options.DisplayOperationId();            
        });
    }

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