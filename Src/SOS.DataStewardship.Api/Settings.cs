using System.Text.Json;
public interface ISettings
{
    //string[] AllowedOrigins { get; init; }

    ApplicationInsights ApplicationInsights { get; init; }
    IdentityServerConfiguration IdentityServer {get; init;}
    MongoDbConfiguration ProcessDbConfiguration { get; init; }
    ElasticSearchConfiguration SearchDbConfiguration { get; init; }
    UserServiceConfiguration UserServiceConfiguration { get; init; }
    string InstrumentationKey { get; init; }
    string ProcessDbUserName {get; init; }
    string ProcessDbPassword {get; init; }
    string SearchDbUserName {get; init; }
    string SearchDbPassword {get; init; }
}
public class Settings : ISettings
{
    //public string[] AllowedOrigins { get; init; } = Array.Empty<string>();

    public ApplicationInsights ApplicationInsights { get; init; } = new();
    public IdentityServerConfiguration IdentityServer { get; init; } = new();
    public MongoDbConfiguration ProcessDbConfiguration { get; init; } = new();
    public ElasticSearchConfiguration SearchDbConfiguration { get; init; } = new();
    public UserServiceConfiguration UserServiceConfiguration { get; init; } = new();
    public string InstrumentationKey { get; init; } = "";
    public string ProcessDbUserName {get; init; } = "";
    public string ProcessDbPassword {get; init; } = "";
    public string SearchDbUserName {get; init; } = "";
    public string SearchDbPassword {get; init; } = "";

    // Prioritizes environment variables but falls back on appsettings.<env>.json and throws if neither are found
    // Logs the source of the setting (environment variable or appsettings.<env>.json) and the setting but redacts sensitive information
    public Settings(IConfiguration configuration) // register in DI container as singleton!
    {
        //AllowedOrigins = GetConfigValueString("AllowedOrigins", configuration).Split(",").Select(s => s.Trim()).ToArray();

        ApplicationInsights = GetConfigSection<ApplicationInsights>("ApplicationInsights", configuration);
        InstrumentationKey = GetConfigValueString("InstrumentationKey", configuration, sensitiveSetting: true, required: false);
        if (ApplicationInsights.InstrumentationKey.Contains("SECRET_PLACEHOLDER"))
        {
            ApplicationInsights.InstrumentationKey = InstrumentationKey;
            Log.Logger.Information("replaced SECRET_PLACEHOLDER in ApplicationInsights.InstrumentationKey with the value in InstrumentationKey");
        }

        IdentityServer = GetConfigSection<IdentityServerConfiguration>("IdentityServer", configuration);
        ProcessDbConfiguration = GetConfigSection<MongoDbConfiguration>("ProcessDbConfiguration", configuration);
        ProcessDbUserName = GetConfigValueString("ProcessDbUserName", configuration, sensitiveSetting: true, required: false);
        if (ProcessDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            ProcessDbConfiguration.UserName = ProcessDbUserName;
            Log.Logger.Information("replaced SECRET_PLACEHOLDER in ProcessDbConfiguration.UserName with the value in ProcessDbUserName");
        }
        ProcessDbPassword = GetConfigValueString("ProcessDbPassword", configuration, sensitiveSetting: true, required: false);
        if (ProcessDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            ProcessDbConfiguration.Password = ProcessDbPassword;
            Log.Logger.Information("replaced SECRET_PLACEHOLDER in ProcessDbConfiguration.Password with the value in ProcessDbPassword");
        }
        SearchDbConfiguration = GetConfigSection<ElasticSearchConfiguration>("SearchDbConfiguration", configuration);
        SearchDbUserName = GetConfigValueString("SearchDbUserName", configuration, sensitiveSetting: true, required: false);
        if (SearchDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            SearchDbConfiguration.UserName = SearchDbUserName;
            Log.Logger.Information("replaced SECRET_PLACEHOLDER in SearchDbConfiguration.UserName with the value in SearchDbUserName");
        }
        SearchDbPassword = GetConfigValueString("SearchDbPassword", configuration, sensitiveSetting: true, required: false);
        if (SearchDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            SearchDbConfiguration.Password = SearchDbPassword;
            Log.Logger.Information("replaced SECRET_PLACEHOLDER in SearchDbConfiguration.Password with the value in SearchDbPassword");
        }
        UserServiceConfiguration = GetConfigSection<UserServiceConfiguration>("UserServiceConfiguration", configuration);
    }

    private static T GetConfigSection<T>(string key, IConfiguration configuration, bool sensitiveSetting = false)
    {
        var envValueStr = Environment.GetEnvironmentVariable(key);
        var envValue = !string.IsNullOrEmpty(envValueStr) ? JsonSerializer.Deserialize<T>(envValueStr) : default;
        if (envValue != null)
        {
            LogConfigValue(key, JsonSerializer.Serialize(envValue), "environment variable (check /devops/k8s/<env>/<manifest.yaml>)", sensitiveSetting);
            return envValue;
        }
       
        var confValue = configuration.GetSection(key).Get<T>();
        if (confValue != null)
        {
            LogConfigValue(key, JsonSerializer.Serialize(confValue), "appsettings.<env>.json", sensitiveSetting);
            return confValue;
        }

        throw new Exception($"value for {key} is null or empty!");

    }
    private static string GetConfigValueString(string key, IConfiguration configuration, bool sensitiveSetting = false, bool required = true)
    {
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(envValue))
        {
            LogConfigValue(key, envValue, "environment variable (check /devops/k8s/<env>/<manifest.yaml>)", sensitiveSetting);
            return envValue;
        }

        var confValue = configuration[key];
        if (!String.IsNullOrEmpty(confValue))
        {
            LogConfigValue(key, confValue, "appsettings.<env>.json", sensitiveSetting);
            return confValue;
        }

        if (required)
        {
            throw new Exception($"value for required {key} is null or empty!");
        }
        else
        {
            Log.Logger.Information("Ignoring non-required config value {key}", key);
            return String.Empty;
        }

    }
    private static void LogConfigValue(string key, string value, string source, bool sensitiveSetting = false)
    {

        if (sensitiveSetting)
        {
            Log.Logger.Information($"setting {key} to <redacted> from {source}");
        }
        else
        {
            Log.Logger.Information($"setting {key} to {value} from {source}");
        }
    }
}
