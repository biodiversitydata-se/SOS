using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace SOS.Status.Web.Extensions;

internal static partial class CookieOidcServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection ConfigureCookieOidc(string cookieScheme, string oidcScheme)
        {
            services.AddSingleton<CookieOidcRefresher>();
            services.AddOptions<CookieAuthenticationOptions>(cookieScheme).Configure<CookieOidcRefresher>((cookieOptions, refresher) =>
            {
                cookieOptions.Events.OnValidatePrincipal = context => refresher.ValidateOrRefreshCookieAsync(context, oidcScheme);
            });
            services.AddOptions<OpenIdConnectOptions>(oidcScheme).Configure(oidcOptions =>
            {
                // Request a refresh_token.
                oidcOptions.Scope.Add(OpenIdConnectScope.OfflineAccess);
                // Store the refresh_token.
                oidcOptions.SaveTokens = true;
            });
            return services;
        }
    }
}