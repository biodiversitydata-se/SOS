namespace SOS.DataStewardship.Api.Extensions;

internal static class ExceptionMiddlewareExtension
{
    internal static void ConfigureExceptionHandler(this IApplicationBuilder app, Logger logger, bool isDevelopment)
    {
        app.UseExceptionHandler(error =>
        {
            error.Run(async context =>
            {
                var errorAsString = string.Empty;
                var contextFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                if (contextFeature != null)
                {
                    var exception = contextFeature.Error;
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorAsString = exception.Message;
                    logger.Error(exception, errorAsString);
                }
                else
                {
                    errorAsString = "An error occurred";
                }
                                                
                await Results.Problem(
                    title: "An error occurred while processing your request.",
                    detail: isDevelopment ? errorAsString : string.Empty,
                    type: "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    statusCode: 500).ExecuteAsync(context);
            });
        });
    }
}
