
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
    await ConfigureServicesAsync(
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

static async Task ConfigureServicesAsync(
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
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    if (!string.IsNullOrEmpty(Settings.RedisConfiguration?.EndPoint))
    {
        var redisConnection = await ConnectionMultiplexer.ConnectAsync(ConfigurationOptions.Parse($"{Settings.RedisConfiguration.EndPoint}:{Settings.RedisConfiguration.Port},password={Settings.RedisConfiguration.Password},serviceName={Settings.RedisConfiguration.ServiceName},allowAdmin=true"));
        Log.Logger.Information($"Redis connected: {redisConnection?.IsConnected ?? false}");
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
            options.RequireHttpsMetadata = false;
            options.Authority = Settings.AuthenticationConfiguration.Authority;
            options.ClientId = Settings.AuthenticationConfiguration.ClientId;
            options.ResponseType = "code";
            options.SaveTokens = false; // Important to work in k8s
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("roles");
            /* Enable to get extra logging
            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = context =>
                {
                    // Här kan du logga ut token för debugging
                    var idToken = context.SecurityToken.RawData;
                    Log.Information("OIDC id_token: {IdToken}", idToken);

                    var accessToken = context.TokenEndpointResponse?.AccessToken;
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        Log.Information("OIDC access_token: {AccessToken}", accessToken);
                    }

                    return Task.CompletedTask;
                },
                OnRemoteFailure = context =>
                {
                    Log.Error(context.Failure, "OIDC Remote Failure");
                    context.HandleResponse();
                    context.Response.Redirect("/error?message=" + Uri.EscapeDataString(context.Failure?.Message));
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Log.Error(context.Exception, "OIDC Auth Failed");
                    context.HandleResponse(); // ← superviktigt, annars kastas felet vidare = 502
                    context.Response.Redirect("/error?message=" + Uri.EscapeDataString(context.Exception.Message));
                    return Task.CompletedTask;
                },
                OnTokenResponseReceived = context =>
                {
                    Log.Information("OIDC Token response received");
                    return Task.CompletedTask;
                }
            };*/
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
    /* Extra loging
      app.Use(async (context, next) =>
    {
        var method = context.Request.Method;
        var path = context.Request.Path;
        var contentLength = context.Request.ContentLength?.ToString() ?? "null";
        var contentType = context.Request.ContentType ?? "null";

        Log.Information($"[REQ] {method} {path} | Content-Length: {contentLength} | Content-Type: {contentType}");

        await next.Invoke();
    });*/
    app.UseHttpsRedirection();
    app.UseRouting();
    app.ApplyMapHealthChecks();
    app.UseAuthentication();
    app.UseAuthorization();

    app.ApplyUseSerilogRequestLogging();    

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
            Authorization = new[] { new HangfireAuthorizationFilter() },
            AppPath = "/logout", // You can use this to make the "Back to site" link go to logout
        })
        .RequireAuthorization("SOS_ADMIN_POLICY"); // This replaces standard authorization filter
    }

    if (isDevelopment)
        app.UseDeveloperExceptionPage();
    else
        app.UseHsts();

    app.PreventSwaggerCaching();
    app.ApplyUseSwagger();
    app.MapControllers();
}

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    /// <summary>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public bool Authorize(DashboardContext context)
    {
        return true; // Always return true since authorization is handled by SOS_ADMIN_POLICY
    }
}