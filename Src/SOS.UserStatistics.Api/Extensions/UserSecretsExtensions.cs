using System.Reflection;

namespace SOS.UserStatistics.Api.Extensions
{
    internal static class UserSecretsExtensions
    {
        public static WebApplicationBuilder SetupUserSecrets(this WebApplicationBuilder builder)
        {
            builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            return builder;
        }
    }
}
