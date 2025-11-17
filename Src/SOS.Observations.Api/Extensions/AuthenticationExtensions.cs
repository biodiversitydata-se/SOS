using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Extensions;

public static class AuthenticationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection SetupAuthentication()
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
}