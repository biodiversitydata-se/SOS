namespace SOS.Status.Web.Extensions;

public static class SecurityHeadersExtensions
{
    /// <summary>
    /// Adds common security-related HTTP headers to all responses in the application pipeline.
    /// </summary>
    /// <remarks>This method configures headers such as Content-Security-Policy, Permissions-Policy,
    /// Referrer-Policy, X-Content-Type-Options, X-Frame-Options, and X-XSS-Protection to enhance the security of HTTP
    /// responses. The Content-Security-Policy and connect-src directives are adjusted based on the value of <paramref
    /// name="secure"/>. Call this method early in the middleware pipeline to ensure headers are applied to all
    /// responses.</remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance to which the security headers middleware is added.</param>
    /// <param name="secure">true to configure headers for secure (HTTPS and WSS) connections; otherwise, false to allow HTTP and WS
    /// connections.</param>
    /// <returns>The <see cref="WebApplication"/> instance with the security headers middleware configured.</returns>
    public static WebApplication AddSecurityHeaders(this WebApplication app, bool secure)
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers["Content-Security-Policy"] = $"default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self' {(secure ? "https:" : "http:")} {(secure ? "wss:" : "ws:")}; object-src 'none'; frame-ancestors 'none'; upgrade-insecure-requests";
            context.Response.Headers["Permissions-Policy"] = "geolocation=(),  camera=(),  microphone=(),  payment=(),  usb=(),  accelerometer=(),  gyroscope=(),  magnetometer=()";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "0"; // deprecated but sometimes required

            await next();
        });

        return app;
    }
}
