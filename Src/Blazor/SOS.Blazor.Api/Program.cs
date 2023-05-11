var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var logger = NLogBuilder.ConfigureNLog($"nlog.{env}.config").GetCurrentClassLogger();
logger.Info("Starting application...");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.SetupLogging();
    builder.SetupDependencies();
    builder.RegisterModules();
    builder.Services.AddRazorPages();

    var app = builder.Build();
    app.ConfigureExceptionHandler(logger, app.Environment.IsDevelopment());
    app.MapEndpoints();
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    app.UseRouting();

    app.MapRazorPages();
    app.MapFallbackToFile("index.html");

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


