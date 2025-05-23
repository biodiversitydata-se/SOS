using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace SOS.Analysis.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection SetupAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Audience = Settings.UserServiceConfiguration.IdentityProvider.Audience;
                options.Authority = Settings.UserServiceConfiguration.IdentityProvider.Authority;
                options.RequireHttpsMetadata = Settings.UserServiceConfiguration.IdentityProvider.RequireHttpsMetadata;
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        var scopeClaim = claimsIdentity?.FindFirst("scope");
                        if (claimsIdentity != null && scopeClaim != null)
                        {
                            var scopes = scopeClaim.Value.Split(' ');
                            claimsIdentity.RemoveClaim(scopeClaim);
                            foreach (var scope in scopes)
                            {
                                claimsIdentity.AddClaim(new Claim("scope", scope));
                            }
                        }
                        return Task.CompletedTask;
                    }
                };
            });      

        return services;
    }
}