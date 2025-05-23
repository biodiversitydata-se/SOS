using Microsoft.IdentityModel.JsonWebTokens;
using Serilog;
using Serilog.Filters;
using Serilog.Formatting.Compact;

namespace SOS.ElasticSearch.Proxy.Extensions;

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

    public static WebApplication ApplyUseSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.Items.TryGetValue("UserId", out var userId))
                {
                    diagnosticContext.Set("UserId", userId);
                }

                if (httpContext.Items.TryGetValue("Email", out var email))
                {
                    diagnosticContext.Set("Email", email);
                }

                if (httpContext.Items.TryGetValue("Endpoint", out var endpoint))
                {
                    diagnosticContext.Set("Endpoint", endpoint);
                }

                if (httpContext.Items.TryGetValue("QueryString", out var queryString))
                {
                    diagnosticContext.Set("QueryString", queryString);
                }

                if (httpContext.Items.TryGetValue("Handler", out var handler))
                {
                    diagnosticContext.Set("Handler", handler);
                }

                if (httpContext.Items.TryGetValue("ApiUserType", out var apiUserType))
                {
                    diagnosticContext.Set("ApiUserType", apiUserType);
                }

                if (httpContext.Items.TryGetValue("SemaphoreStatus", out var semaphoreStatus))
                {
                    diagnosticContext.Set("SemaphoreStatus", semaphoreStatus);
                }

                if (httpContext.Items.TryGetValue("SemaphoreWaitSeconds", out var semaphoreWaitSeconds))
                {
                    diagnosticContext.Set("SemaphoreWaitSeconds", semaphoreWaitSeconds);
                }

                try
                {
                    var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                    if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        string token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);
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
