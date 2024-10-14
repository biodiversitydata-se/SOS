namespace SOS.DataStewardship.Api.Extensions;

internal static class AuthenticationExtensions
{
    internal static WebApplicationBuilder SetupAuthentication(this WebApplicationBuilder webApplicationBuilder, ISettings settings)
    {
        var identityServerConfiguration = settings.IdentityServer;
        webApplicationBuilder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Audience = identityServerConfiguration.Audience;
                    options.Authority = identityServerConfiguration.Authority;
                    options.RequireHttpsMetadata = identityServerConfiguration.RequireHttpsMetadata;
                    options.TokenValidationParameters.RoleClaimType = "rname";
                });
        return webApplicationBuilder;
    }
}
