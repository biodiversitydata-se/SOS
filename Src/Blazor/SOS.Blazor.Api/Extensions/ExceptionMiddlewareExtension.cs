namespace SOS.Blazor.Api.Extensions;

internal static class ExceptionMiddlewareExtension
{
    internal static void ConfigureExceptionHandler(this IApplicationBuilder app, bool isDevelopment)
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
                }
                else
                {
                    errorAsString = "An error occurred";
                }

                await context.Response.WriteAsync(isDevelopment ? errorAsString : string.Empty);
            });
        });
    }
}
