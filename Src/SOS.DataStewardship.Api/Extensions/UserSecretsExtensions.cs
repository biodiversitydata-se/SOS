namespace SOS.DataStewardship.Api.Extensions;

public static class UserSecretsExtensions
{
    extension(WebApplicationBuilder webApplicationBuilder)
    {
        public WebApplicationBuilder SetupUserSecrets()
        {
            webApplicationBuilder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            return webApplicationBuilder;
        }
    }
}
