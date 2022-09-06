using SOS.UserStatistics.Api.Extensions;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var logger = NLogBuilder.ConfigureNLog($"NLog.{env}.config").GetCurrentClassLogger();
logger.Info("Starting application...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.SetupLogging();
    builder.Host.UseNLog();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.SetupUserSecrets();
    builder.SetupDependencies();
    builder.RegisterModules();

    var app = builder.Build();
    app.UseHttpsRedirection();

    if (app.Environment.IsEnvironment("local"))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

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