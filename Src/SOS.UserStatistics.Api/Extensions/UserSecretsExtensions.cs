namespace SOS.UserStatistics.Api.Extensions;

internal static class UserSecretsExtensions
{
    internal static WebApplicationBuilder SetupUserSecrets(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
        return webApplicationBuilder;
    }
}
