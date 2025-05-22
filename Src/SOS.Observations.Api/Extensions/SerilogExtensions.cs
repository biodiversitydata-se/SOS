using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.JsonWebTokens;
using Serilog;
using System;
using System.Linq;

namespace SOS.Observations.Api.Extensions;

public static class SerilogExtensions
{
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

                string originalToken = string.Empty;
                try
                {
                    var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                    originalToken = authHeader;
                    if (authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        string token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
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
                    Log.Logger.Error(ex, "Error when deserializing JWT. Token={token}", originalToken);
                }
            };
        });

        return app;
    }
}
