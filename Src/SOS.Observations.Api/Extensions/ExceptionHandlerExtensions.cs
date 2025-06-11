using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace SOS.Observations.Api.Extensions;

public static class ExceptionHandlerExtensions
{
    public static WebApplication ApplyUseExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;

                if (exception is JsonException || exception?.InnerException is JsonException || exception.Source == "NetTopologySuite")
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = "application/json";
                    string errorMsg = "Invalid JSON in request body.";
                    if (exception.Source == "NetTopologySuite")
                    {
                        errorMsg += $" Geometry - {exception?.Message}";
                    }
                    else
                    {
                        errorMsg += $" {exception?.Message}.";
                    }

                    await context.Response.WriteAsync(errorMsg);
                    return;
                }

                // Otherwise return 500
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An internal server error has occurred.");
            });
        });

        return app;
    }
}
