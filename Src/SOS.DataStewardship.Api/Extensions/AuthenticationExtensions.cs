namespace SOS.DataStewardship.Api.Extensions;

internal static class AuthenticationExtensions
{
    extension(WebApplicationBuilder webApplicationBuilder)
    {
        internal WebApplicationBuilder SetupAuthentication(ISettings settings)
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
}
