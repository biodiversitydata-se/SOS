using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using System;
using System.Linq;

namespace SOS.Status.Web.Extensions;

public static class SerilogExtensions
{
    public static void SetupSerilog(bool isLocalDevelopment)
    {
        Log.Logger = isLocalDevelopment ?
            new LoggerConfiguration() // human readable in the terminal when developing, not all json
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj} {Properties}{NewLine}{Exception}")
            .CreateLogger()
        :
            new LoggerConfiguration() // compact json when running in the clusters for structured logging
                .MinimumLevel.Information()
                .WriteTo.Console(new RenderedCompactJsonFormatter())
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.Result", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.HttpResults", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Cors.Infrastructure", Serilog.Events.LogEventLevel.Warning)
                .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", p => p == "/healthz"))
            .CreateLogger();
    }

    extension(WebApplication app)
    {
        public WebApplication ApplyUseSerilogRequestLogging()
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
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            string token = authHeader["Bearer ".Length..]; // enklare än Replace
                            var jsonWebTokenHandler = new JsonWebTokenHandler();
                            var jwt = jsonWebTokenHandler.ReadJsonWebToken(token);

                            if (jwt != null)
                            {
                                // Sätt ClientId om den finns
                                var clientId = jwt.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
                                if (!string.IsNullOrEmpty(clientId))
                                    diagnosticContext.Set("ClientId", clientId);

                                // Sätt Name om den finns
                                var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
                                if (!string.IsNullOrEmpty(name))
                                    diagnosticContext.Set("Name", name);

                                // Sätt Subject om den finns
                                if (!string.IsNullOrEmpty(jwt.Subject))
                                    diagnosticContext.Set("Subject", jwt.Subject);
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
}