using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SOS.Status.Web.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection SetupAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "oidc";
        })
        .AddCookie("Cookies", options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // require HTTPS
        })
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = Settings.UserServiceConfiguration.IdentityProvider.Authority;
            options.ClientId = Settings.UserServiceConfiguration.ClientId;
            options.ResponseType = "code";

            options.SaveTokens = true;
            options.Scope.Clear();
            foreach (var scope in Settings.UserServiceConfiguration.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                options.Scope.Add(scope);
            }

            options.CallbackPath = "/signin-oidc";
            options.SignedOutCallbackPath = "/signout-callback-oidc";
        });

        return services;
    }

    public static WebApplication MapLoginEndpoint(this WebApplication app)
    {
        app.MapGet("/authentication/login", async (HttpContext context) =>
        {
            var returnUrl = context.Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("/authentication/login"))
            {
                returnUrl = "/";
            }

            await context.ChallengeAsync("oidc", new AuthenticationProperties
            {
                RedirectUri = returnUrl
            });
        });

        return app;
    }

    public static WebApplication MapLogoutEndpoint(this WebApplication app)
    {
        app.MapGet("/signout", async (HttpContext context) =>
        {
            var returnUrl = context.Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }

            await context.SignOutAsync("Cookies");
            await context.SignOutAsync("oidc", new AuthenticationProperties
            {
                RedirectUri = returnUrl
            });
        });

        return app;
    }
}