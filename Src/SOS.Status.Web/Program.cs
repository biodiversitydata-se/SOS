using Blazored.LocalStorage;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor;
using MudBlazor.Services;
using Serilog;
using SOS.Lib.Helpers;
using SOS.Status.Web.Client.Abstractions;
using SOS.Status.Web.Client.JsonConverters;
using SOS.Status.Web.Client.Models;
using SOS.Status.Web.Components;
using SOS.Status.Web.Endpoints;
using SOS.Status.Web.Extensions;
using System.Globalization;
using System.Text.Json.Serialization;

// --- Program startup ---

try
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    bool isLocalDevelopment = new[] { "local", "development", "k8s" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool isDevelopment = new[] { "local", "development", "dev", "st" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);

    // Setup logging
    SerilogExtensions.SetupSerilog(isDevelopment);
    Log.Logger.Information("Starting Service");

    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults();
    SeriLogHelper.ConfigureSerilog(builder);
    var configurationRoot = BuildConfiguration(builder, isDevelopment);
    Settings.Init(configurationRoot);
    builder.Services.AddMudServices();
    builder.Services.AddMudMarkdownServices();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents()
        .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

    //builder.Services.AddHttpClient<StatusInfoApiClient>(client =>
    //{
    //    client.BaseAddress = new Uri("http://localhost:5006");
    //});

    builder.Services.AddDependencyInjectionServices(configurationRoot);
    builder.Services.AddSingleton<IAppEnvironment>(sp => new AppEnvironment(builder.Environment.EnvironmentName));
    builder.Services.SetupAuthentication();
    builder.Services.AddAuthorization();    
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
    builder.Services.AddHealthChecks();    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazorClient", policy =>
        {
            policy.WithOrigins("http://localhost:5006")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });

        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
    builder.Services.AddBlazoredLocalStorage();
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.SerializerOptions.Converters.Add(new GeoJsonConverter());
    });

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.RequireHeaderSymmetry = false;
        options.ForwardLimit = null;        
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });

    var app = builder.Build();
    app.UseForwardedHeaders();
    app.MapDefaultEndpoints();

    // Use Swedish culture
    var culture = new CultureInfo("sv-SE");
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    app.MapStatusInfoEndpoints();
    app.MapTaxonDiagramEndpoints();
    app.MapObservationSearchEndpoints();

    // Configure the HTTP request pipeline.
    if (isDevelopment)
    {
        app.UseWebAssemblyDebugging();                
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        //The default HSTS value is 30 days.You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    //app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.ApplyUseSerilogRequestLogging();
    app.UseAntiforgery();
    StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
    app.UseStaticFiles();
    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(SOS.Status.Web.Client._Imports).Assembly);

    app.MapLoginEndpoint();
    app.MapLogoutEndpoint();
    app.MapDebugTokenEndpoint();
    app.MapHealthChecks("/healthz", new HealthCheckOptions()
    {
        Predicate = r => r.Tags.Contains("k8s")
    });

    app.UseCors("AllowAll");

    // Start the application
    string? aspnetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    await app.RunAsync(aspnetCoreUrls ?? "http://*:5000");
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
static IConfigurationRoot BuildConfiguration(WebApplicationBuilder builder, bool isDevelopment)
{
    var environment = builder.Environment.EnvironmentName.ToLower();
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{environment}.json", true)
        .AddEnvironmentVariables();
#if DEBUG
    configBuilder.AddUserSecrets<Program>();
#else
    if (isDevelopment)
        configBuilder.AddUserSecrets<Program>();
#endif
    return configBuilder.Build();
}