namespace SOS.UserStatistics.Api.Extensions;

internal static class AuthenticationExtensions
{
    internal static WebApplicationBuilder SetupAuthentication(this WebApplicationBuilder webApplicationBuilder)
    {
        var identityServerConfiguration = webApplicationBuilder.Configuration.GetSection("IdentityServer").Get<IdentityServerConfiguration>();
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
