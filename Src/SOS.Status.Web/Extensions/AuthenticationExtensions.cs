using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Runtime.InteropServices;

namespace SOS.Status.Web.Extensions;

public static class AuthenticationExtensions
{    
    const string OIDC_SCHEME = "oidc";

    public static IServiceCollection SetupAuthentication(this IServiceCollection services)
    {        
        //services.AddAuthentication(options =>
        //{
        //    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = OIDC_SCHEME;
        //})
        services.AddAuthentication(OIDC_SCHEME)
            .AddOpenIdConnect(OIDC_SCHEME, oidcOptions =>
            {                
                oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;                
                oidcOptions.Scope.Clear();
                foreach (var scope in Settings.UserServiceConfiguration.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    oidcOptions.Scope.Add(scope);
                }

                oidcOptions.CallbackPath = new PathString("/signin-oidc");
                oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");                
                oidcOptions.Authority = Settings.UserServiceConfiguration.IdentityProvider.Authority;               
                oidcOptions.ClientId = Settings.UserServiceConfiguration.ClientId;                
                oidcOptions.ResponseType = OpenIdConnectResponseType.Code;                
                oidcOptions.MapInboundClaims = false;
                oidcOptions.TokenValidationParameters.NameClaimType = "name";
                oidcOptions.TokenValidationParameters.RoleClaimType = "role";                
                oidcOptions.SaveTokens = true;
                
                oidcOptions.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
                {
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask; // Just for debug purpose
                    }
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
            //.AddCookie("Cookies", options =>
            //{
            //    options.Cookie.SameSite = SameSiteMode.None;
            //    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // require HTTPS
            //})

        services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, OIDC_SCHEME);

        services.AddAuthorization();

        services.AddCascadingAuthenticationState();

        //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //    .AddJwtBearer(options =>
        //    {
        //        options.Authority = Settings.UserServiceConfiguration.IdentityProvider.Authority;
        //        options.Audience = "ditt-api"; // eller kolla mot scope
        //    });

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
            
            await context.ChallengeAsync(OIDC_SCHEME, new AuthenticationProperties
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
            var returnUrl = context.Request.Query["returnUrl"].ToString();
            //var returnUrl = context.Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }

            await context.SignOutAsync("Cookies");
            await context.SignOutAsync(OIDC_SCHEME, new AuthenticationProperties
            {
                RedirectUri = returnUrl
            });
        });

        return app;
    }

    public static WebApplication MapDebugTokenEndpoint(this WebApplication app)
    {
        app.MapGet("/authentication/debugtoken", async (HttpContext context) =>
        {
            if (!(context.User.Identity?.IsAuthenticated ?? false))
            {
                return Results.Unauthorized();
            }

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var swedenTz = TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "W. Europe Standard Time"  // Windows
                    : "Europe/Stockholm");       // Linux/macOS

            object TokenInfo(string? token, string tokenType)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new
                    {
                        TokenType = tokenType,
                        Error = "Token is null or empty"
                    };
                }

                try
                {
                    var jwt = handler.ReadJwtToken(token);
                    return new
                    {
                        TokenType = tokenType,                        
                        ValidToUtc = jwt.ValidTo,                        
                        ValidToSweden = TimeZoneInfo.ConvertTimeFromUtc(jwt.ValidTo, swedenTz),
                        Raw = token
                    };
                }
                catch (Exception ex)
                {
                    return new
                    {
                        TokenType = tokenType,
                        Error = $"Failed to parse token: {ex.Message}",
                        Raw = token
                    };
                }
            }

            var accessToken = await context.GetTokenAsync("access_token");
            var idToken = await context.GetTokenAsync("id_token");
            var refreshToken = await context.GetTokenAsync("refresh_token");

            var tokens = new[]
            {
                TokenInfo(accessToken, "AccessToken"),
                TokenInfo(idToken, "IdToken"),
                TokenInfo(refreshToken, "RefreshToken")
            };

            return Results.Ok(new
            {
                User = context.User.Identity?.Name,
                Claims = context.User.Claims.Select(c => new { c.Type, c.Value }),
                Tokens = tokens
            });
        });

        return app;
    }
}