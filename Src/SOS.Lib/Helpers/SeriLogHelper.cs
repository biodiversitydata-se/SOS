using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog;
using Serilog.Core;
using Serilog.Filters;
using Serilog.Templates;
using SOS.Lib.Logging;

namespace SOS.Lib.Helpers;
public static class SeriLogHelper
{
    /// <summary>
    /// Configures the specified logger with enrichment properties and output settings based on the development
    /// environment.
    /// </summary>
    /// <remarks>When not in local development, the configuration applies log level overrides for specific
    /// Microsoft namespaces and excludes health check and metrics request paths from logging. This helps reduce log
    /// noise in production environments.</remarks>
    /// <param name="configuration">The logger configuration to apply enrichment and output settings to. Cannot be null.</param>
    /// <param name="isLocalDevelopment">A value indicating whether the configuration should be tailored for local development. If <see
    /// langword="true"/>, debug-level logging and console output are enabled; otherwise, information-level logging and
    /// additional filters are applied.</param>
    private static void ApplyConfiguration(LoggerConfiguration configuration, bool isLocalDevelopment)
    {
        configuration
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Team", "Pantera")
            .Enrich.With<AlwaysIncludeLevelEnricher>();

        if (isLocalDevelopment)
        {
            configuration
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Debug();
        }
        else
        {
            configuration
                .MinimumLevel.Information()
                .WriteTo.Console(new ExpressionTemplate(
                     "{ {timestamp: @t, message: @m, level: if @l is null then 'Info' else @l, exception: @x, ..@p} }\n")
                )
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.Result", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.HttpResults", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Cors.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/health"))
                .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/health-json"))
                .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/healthz"))
                .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/metrics"));
        }
    }

    public static Logger CreateLogger(bool isLocalDevelopment)
    {
        var configuration = new LoggerConfiguration();
        ApplyConfiguration(configuration, isLocalDevelopment);
        return configuration.CreateLogger();
    }

    /// <summary>
    /// Configures Serilog logging for the specified web application builder, including OpenTelemetry integration and
    /// environment-specific settings.
    /// </summary>
    /// <remarks>Serilog is configured to read settings from the application's configuration and, if
    /// OpenTelemetry endpoints are specified, to forward logs with resource attributes. Ensure that relevant
    /// OpenTelemetry configuration values are present in the application's configuration for proper log
    /// export.</remarks>
    /// <param name="builder">The web application builder to configure Serilog logging for. Must not be null.</param>
    /// <param name="serviceName">The name of the service to be used as a resource attribute for OpenTelemetry logs.</param>
    /// <param name="isLocalDevelopment">Indicates whether the configuration should apply local development settings. Set to <see langword="true"/> for
    /// local development; otherwise, <see langword="false"/>.</param>
    public static void ConfigureSerilog(WebApplicationBuilder builder, string serviceName, bool isLocalDevelopment)
    {
        builder.Host.UseSerilog((ctx, lc) =>
        {
            ApplyConfiguration(lc, isLocalDevelopment);
           
            // Configure OpenTelemetry sink
            var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            if (!string.IsNullOrEmpty(otlpEndpoint))
            {
                lc.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    
                    var headers = builder.Configuration["OTEL_EXPORTER_OTLP_HEADERS"]?.Split(',') ?? [];
                    foreach (var header in headers)
                    {
                        var parts = header.Split('=');
                        if (parts.Length == 2)
                        {
                            options.Headers.Add(parts[0], parts[1]);
                        }
                    }
                    
                    options.ResourceAttributes.Add("service.name", serviceName);

                    var otelResourceAttributes = builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"];
                    if (!string.IsNullOrEmpty(otelResourceAttributes))
                    {
                        var parts = otelResourceAttributes.Split('=');
                        if (parts.Length == 2)
                        {
                            options.ResourceAttributes.Add(parts[0], parts[1]);
                        }
                    }
                });
            }

            lc.ReadFrom.Configuration(ctx.Configuration);
        });
    }

    /// <summary>
    /// Configures Serilog request logging for the specified web application, enriching log events with additional
    /// context from HTTP requests.
    /// </summary>
    /// <remarks>This method adds Serilog request logging middleware to the application pipeline. Log events
    /// are enriched with values from the HTTP context, such as user information, endpoint details, and JWT claims, if
    /// available. Use this extension to provide more detailed request logs for diagnostics and monitoring.</remarks>
    /// <param name="app">The web application instance to configure Serilog request logging for.</param>
    /// <returns>The same web application instance, with Serilog request logging enabled and enriched with custom diagnostic
    /// context.</returns>
    public static WebApplication ApplyUseSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.Items.TryGetValue("UserId", out var userId) && userId != null)
                {
                    diagnosticContext.Set("UserId", userId);
                }

                if (httpContext.Items.TryGetValue("Email", out var email) && email != null)
                {
                    diagnosticContext.Set("Email", email);
                }

                if (httpContext.Items.TryGetValue("Endpoint", out var endpoint) && endpoint != null)
                {
                    diagnosticContext.Set("Endpoint", endpoint);
                }

                if (httpContext.Items.TryGetValue("QueryString", out var queryString) && queryString != null)
                {
                    diagnosticContext.Set("QueryString", queryString);
                }

                if (httpContext.Items.TryGetValue("Handler", out var handler) && handler != null)
                {
                    diagnosticContext.Set("Handler", handler);
                }

                if (httpContext.Items.TryGetValue("ApiUserType", out var apiUserType) && apiUserType != null)
                {
                    diagnosticContext.Set("ApiUserType", apiUserType);
                }

                if (httpContext.Items.TryGetValue("SemaphoreStatus", out var semaphoreStatus) && semaphoreStatus != null)
                {
                    diagnosticContext.Set("SemaphoreStatus", semaphoreStatus);
                }

                if (httpContext.Items.TryGetValue("SemaphoreWaitSeconds", out var semaphoreWaitSeconds) && semaphoreWaitSeconds != null)
                {
                    diagnosticContext.Set("SemaphoreWaitSeconds", semaphoreWaitSeconds);
                }

                try
                {
                    var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                    if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        string token = authHeader["Bearer ".Length..]; // enklare än Replace
                        var jsonWebTokenHandler = new JsonWebTokenHandler();
                        var jwt = jsonWebTokenHandler.ReadJsonWebToken(token);
                        if (jwt != null)
                        {
                            string? clientId = jwt.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
                            if (clientId != null) diagnosticContext.Set("ClientId", clientId);
                            string? name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                            if (name != null) diagnosticContext.Set("Name", name);
                            if (jwt.Subject != null) diagnosticContext.Set("Subject", jwt.Subject);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Error when deserializing JWT.");
                }
            };
        });

        return app;
    }
}
