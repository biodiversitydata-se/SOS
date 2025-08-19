
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson.Serialization.Conventions;
using Serilog;
using SOS.Administration.Api.Extensions;
using SOS.Lib.Helpers;
using StackExchange.Redis;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

// --- Program startup ---

try
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    bool isLocalDevelopment = new[] { "local", "k8s" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool isDevelopment = new[] { "local", "dev", "st" }.Contains(env?.ToLower(), StringComparer.CurrentCultureIgnoreCase);
    bool disableHangfireInit = Environment.GetEnvironmentVariable("DISABLE_HANGFIRE_INIT").GetBoolean();
    bool useLocalHangfire = Environment.GetEnvironmentVariable("USE_LOCAL_HANGFIRE").GetBoolean();

    // Setup logging
    SerilogExtensions.SetupSerilog(isDevelopment);
    Log.Logger.Information("Starting Service");

    var builder = WebApplication.CreateBuilder(args);
    //builder.Host.UseSerilog(Log.Logger);
    builder.AddServiceDefaults();
    SeriLogHelper.ConfigureSerilog(builder);

    // Set Swedish culture globally
    SetCulture("sv-SE");

    // Register MongoDB conventions
    RegisterMongoConventions();

    // Build configuration
    var configurationRoot = BuildConfiguration(builder, isDevelopment);
    Settings.Init(configurationRoot);

    // Register services
    ConfigureServices(
        builder,
        configurationRoot,
        isDevelopment,
        disableHangfireInit,
        useLocalHangfire);

    // Build app and configure middleware pipeline
    var app = builder.Build();
    app.MapDefaultEndpoints();
    ConfigureMiddleware(app, isDevelopment, disableHangfireInit);

    // Start the application    
    string aspnetCoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    await app.RunAsync(aspnetCoreUrls ?? "http://*:5005");
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

static void SetCulture(string cultureName)
{
    var culture = new CultureInfo(cultureName);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}

static void RegisterMongoConventions()
{
    ConventionRegistry.Register(
        "MongoDB Solution Conventions",
        new ConventionPack { new IgnoreExtraElementsConvention(true), new IgnoreIfNullConvention(true) },
        t => true);
}

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

static void ConfigureServices(
    WebApplicationBuilder builder,
    IConfigurationRoot configuration,
    bool isDevelopment,
    bool disableHangfireInit,
    bool useLocalHangfire)
{
    IServiceCollection services = builder.Services;

    services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.RequireHeaderSymmetry = false;
        options.ForwardLimit = null;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    if (!string.IsNullOrEmpty(Settings.RedisConfiguration?.EndPoint))
    {
        var redisConfiguration = new ConfigurationOptions
        {
            AllowAdmin = true,
            CommandMap = CommandMap.Default,
            EndPoints = { $"{Settings.RedisConfiguration.EndPoint}:{Settings.RedisConfiguration.Port}" },
            Password = Settings.RedisConfiguration.Password
        };
        Log.Logger.Information("Connecting to Redis at {Host}:{Port}:(Length)", Settings.RedisConfiguration.EndPoint, Settings.RedisConfiguration.Port, Settings.RedisConfiguration.Password?.Length);
        var redisConnection = ConnectionMultiplexer.Connect(redisConfiguration);
        services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(redisConnection, "DataProtection-Keys")
            .SetApplicationName("SOSAdminAPI");
    }
   

    services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
        .AddCookie()
        .AddOpenIdConnect(options =>
        {
            options.RequireHttpsMetadata = !isDevelopment;
            options.Authority = Settings.AuthenticationConfiguration.Authority;
            options.ClientId = Settings.AuthenticationConfiguration.ClientId;
            options.ResponseType = "code";
            options.SaveTokens = true;
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("roles");
            options.Events = new OpenIdConnectEvents
            {
                OnRemoteFailure = context =>
                {
                    // Log the exception message to help debug
                    var error = context.Failure?.Message;
                    Console.WriteLine($"OpenID Connect Remote Failure: {error}");
                    context.HandleResponse(); // Prevent default error handling
                    context.Response.Redirect("/error?message=" + Uri.EscapeDataString(error));
                    return Task.CompletedTask;
                }
            };
        });

    services.AddAuthorization(options =>
    {
        options.AddPolicy("SOS_ADMIN_POLICY", policy =>
            policy.RequireRole("SOS-ADMIN"));
    });

    services.AddDependencyInjectionServices();
    services.AddMemoryCache();
    services.AddControllers(options =>
    {
        var policy = new AuthorizationPolicyBuilder() 
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
    services.SetupSwagger();
    if (!disableHangfireInit)
    {
        string hangfireDbConnectionString = builder.Configuration.GetConnectionString("hangfire-mongodb"); // Get Hangfire MongoDB from .Net Aspire configuration.
        services.SetupHangfire(useLocalHangfire, hangfireDbConnectionString);
    }

    services.SetupHealthchecks();
}

static void ConfigureMiddleware(WebApplication app, bool isDevelopment, bool disableHangfireInit)
{
    app.UseForwardedHeaders();
    app.UseHttpsRedirection(); 
    app.UseRouting();
    app.UseAuthentication(); 
    app.UseAuthorization();

    app.ApplyUseSerilogRequestLogging();
    app.ApplyMapHealthChecks();

    app.MapGet("/login", async context =>
    {
        await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = "/hangfire"
        });
    }).AllowAnonymous();
    app.MapGet("/logout", async context =>
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Clear local cookie
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
        {
            RedirectUri = "/hangfire"
        });
    });
    app.MapGet("/error", async context =>
    {
        var message = context.Request.Query["message"].ToString();
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync($"Authentication error:\n{message}");
    }).AllowAnonymous(); ;

    if (!disableHangfireInit)
    {
        app.MapHangfireDashboard("/hangfire", new DashboardOptions
        {
            AppPath = "/logout", // You can use this to make the "Back to site" link go to logout
        })
        .RequireAuthorization("SOS_ADMIN_POLICY"); // This replaces standard authorization filter
    }

    if (isDevelopment)
        app.UseDeveloperExceptionPage();
    else
        app.UseHsts();

    app.ApplyUseSwagger();
    app.MapControllers();
}

public class AllowAllConnectionsFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}