using Autofac.Core;
using SOS.DataStewardship.Api.Extensions;
using SOS.Lib.JsonConverters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    builder.Services.Configure<JsonOptions>(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

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