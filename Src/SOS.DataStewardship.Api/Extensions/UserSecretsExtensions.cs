namespace SOS.DataStewardship.Api.Extensions;

public static class UserSecretsExtensions
{
    public static WebApplicationBuilder SetupUserSecrets(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
        return webApplicationBuilder;
    }
}
