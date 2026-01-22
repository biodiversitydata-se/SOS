using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace SOS.Shared.Api.Middleware;

/// <summary>
/// Middleware that adds essential HTTP security headers to REST API responses and removes server identification 
/// headers to help protect against information disclosure.
/// </summary>
/// <remarks>This middleware sets headers such as X-Content-Type-Options, Referrer-Policy, and optionally 
/// Strict-Transport-Security to enhance API security. It removes server identification headers to prevent 
/// fingerprinting. Integrate this middleware early in the pipeline to ensure headers are set before other 
/// components modify the response.</remarks>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly bool _includeHsts;

    /// <summary>
    /// Initializes a new instance of the SecurityHeadersMiddleware class, which adds security-related HTTP headers to
    /// REST API responses.
    /// </summary>
    /// <remarks>Use this middleware to enhance the security of REST API responses by adding essential headers
    /// and removing server identification. Place it early in the pipeline to ensure headers are set before other 
    /// middleware or endpoints process the response.</remarks>
    /// <param name="next">The next middleware delegate in the request pipeline. Cannot be null.</param>
    /// <param name="includeHsts">Specifies whether the middleware should include the HTTP Strict Transport Security (HSTS) header in responses.
    /// Set to <see langword="true"/> to include HSTS for production environments; otherwise, <see langword="false"/> for development.</param>
    public SecurityHeadersMiddleware(RequestDelegate next, bool includeHsts)
    {
        _next = next;
        _includeHsts = includeHsts;
    }

    /// <summary>
    /// Processes the HTTP request by removing server identification headers and adding essential security headers 
    /// optimized for REST API responses.
    /// </summary>
    /// <remarks>This method removes Server and X-Powered-By headers to prevent server fingerprinting, and adds 
    /// X-Content-Type-Options to prevent MIME-type sniffing and Referrer-Policy to control referer information. 
    /// The Strict-Transport-Security header is added when <c>includeHsts</c> is enabled, enforcing HTTPS for 
    /// production environments.</remarks>
    /// <param name="context">The HTTP context for the current request. Provides access to request and response information.</param>
    /// <returns>A task that represents the asynchronous operation of processing the HTTP request.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Remove server identification headers to prevent fingerprinting
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // Add essential security headers for REST APIs
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // HSTS - enforce HTTPS in production environments
        if (_includeHsts)
        {
            context.Response.Headers.Append("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains");
        }

        await _next(context);
    }
}

/// <summary>
/// Provides extension methods for registering the security headers middleware in an ASP.NET Core REST API application's 
/// request pipeline.
/// </summary>
/// <remarks>Use this class to add essential security-related HTTP headers to REST API responses by invoking the
/// middleware during application startup. This helps protect against information disclosure and enforces secure 
/// communication practices. The middleware should be registered early in the pipeline to ensure headers are
/// applied to all responses.</remarks>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware to the application's request pipeline that sets essential HTTP security headers for REST APIs. 
    /// Optionally includes the HTTP Strict Transport Security (HSTS) header.
    /// </summary>
    /// <remarks>This method should be called early in the middleware configuration to ensure security headers
    /// are set before other middleware processes the response. The HSTS header enforces HTTPS connections and should 
    /// typically only be enabled in production environments.</remarks>
    /// <param name="builder">The application builder used to configure the request pipeline.</param>
    /// <param name="includeHsts">Specifies whether to include the HTTP Strict Transport Security (HSTS) header in responses. Set to <see
    /// langword="true"/> for production to enforce HTTPS; otherwise, <see langword="false"/> for development environments.</param>
    /// <returns>The <see cref="IApplicationBuilder"/> instance with the security headers middleware configured.</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder, bool includeHsts)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>(includeHsts);
    }
}