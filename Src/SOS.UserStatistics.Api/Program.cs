var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var envLog = env != null ? $"nlog.{env}.config" : "nlog.config";
var logger = NLogBuilder.ConfigureNLog(envLog).GetCurrentClassLogger();
logger.Info("Starting application...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.SetupLogging();
    builder.Host.UseNLog();
    builder.Services.AddMemoryCache();
    builder.Services.AddEndpointsApiExplorer();

    builder.SetupSwagger();
    builder.SetupUserSecrets();
    builder.SetupAuthentication();
    builder.SetupDependencies();
    builder.RegisterModules();

    var app = builder.Build();
    app.ConfigureExceptionHandler(logger, app.Environment.IsDevelopment());
    app.MapEndpoints();
    app.UseHttpsRedirection();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
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

public partial class Program { }