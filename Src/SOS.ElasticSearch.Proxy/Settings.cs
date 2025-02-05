using SOS.ElasticSearch.Proxy.Configuration;
using SOS.Lib.Configuration.Shared;
using System.Text.Json;

public static class Settings
{
    public static ApplicationInsights ApplicationInsightsConfiguration { get; set; } = new();
    public static MongoDbConfiguration ProcessDbConfiguration { get; set; } = new();
    public static ProxyConfiguration ProxyConfiguration { get; set; } = new();
    public static ElasticSearchConfiguration SearchDbConfiguration { get; set; } = new();
    public static string InstrumentationKey { get; set; } = "";
    public static string ProcessDbUserName { get; set; } = "";
    public static string ProcessDbPassword { get; set; } = "";
    public static string SearchDbUserName { get; set; } = "";
    public static string SearchDbPassword { get; set; } = "";

    // Prioritizes environment variables but falls back on appsettings.<env>.json and throws if neither are found
    // Logs the source of the setting (environment variable or appsettings.<env>.json) and the setting but redacts sensitive information
    public static void Init(IConfiguration configuration)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Debug).AddConsole());
        var logger = loggerFactory.CreateLogger("Settings");

        // ApplicationInsights
        ApplicationInsightsConfiguration = GetConfigSection<ApplicationInsights>("ApplicationInsights", configuration, logger, sensitiveSetting: true);
        if (ApplicationInsightsConfiguration != null)
        {
            InstrumentationKey = GetConfigValueString("InstrumentationKey", configuration, logger, sensitiveSetting: true, required: false);
            if (ApplicationInsightsConfiguration.InstrumentationKey.Contains("SECRET_PLACEHOLDER"))
            {
                ApplicationInsightsConfiguration.InstrumentationKey = InstrumentationKey;
                logger.LogInformation("replaced SECRET_PLACEHOLDER in ApplicationInsightsConfiguration.InstrumentationKey with the value in InstrumentationKey");
            }
        }

        // Proxy config
        ProxyConfiguration = GetConfigSection<ProxyConfiguration>("ProxyConfiguration", configuration, logger);

        // Process db
        ProcessDbConfiguration = GetConfigSection<MongoDbConfiguration>("ProcessDbConfiguration", configuration, logger, sensitiveSetting: true);
        ProcessDbUserName = GetConfigValueString("ProcessDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (ProcessDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            ProcessDbConfiguration.UserName = ProcessDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ProcessDbConfiguration.UserName with the value in ProcessDbUserName");
        }
        ProcessDbPassword = GetConfigValueString("ProcessDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (ProcessDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            ProcessDbConfiguration.Password = ProcessDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ProcessDbConfiguration.Password with the value in ProcessDbPassword");
        }
        
        // Search db
        SearchDbConfiguration = GetConfigSection<ElasticSearchConfiguration>("SearchDbConfiguration", configuration, logger, sensitiveSetting: true);
        SearchDbUserName = GetConfigValueString("SearchDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (SearchDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            SearchDbConfiguration.UserName = SearchDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in SearchDbConfiguration.UserName with the value in SearchDbUserName");
        }
        SearchDbPassword = GetConfigValueString("SearchDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (SearchDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            SearchDbConfiguration.Password = SearchDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in SearchDbConfiguration.Password with the value in SearchDbPassword");
        }
    }

    private static T GetConfigSection<T>(string key, IConfiguration configuration, ILogger logger, bool sensitiveSetting = false, bool required = true)
    {
        var envValueStr = Environment.GetEnvironmentVariable(key);
        var envValue = !string.IsNullOrEmpty(envValueStr) ? JsonSerializer.Deserialize<T>(envValueStr) : default;
        if (envValue != null)
        {
            LogConfigValue(key, JsonSerializer.Serialize(envValue), "environment variable (check /devops/k8s/<env>/<manifest.yaml>)", logger, sensitiveSetting);
            return envValue;
        }

        var confValue = configuration.GetSection(key).Get<T>();
        if (confValue != null)
        {
            LogConfigValue(key, JsonSerializer.Serialize(confValue), "appsettings.<env>.json", logger, sensitiveSetting);
            return confValue;
        }

        if (required)
            throw new Exception($"value for {key} is null or empty!");
        else
            return default!;
    }
    private static string GetConfigValueString(string key, IConfiguration configuration, ILogger logger, bool sensitiveSetting = false, bool required = true)
    {
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(envValue))
        {
            LogConfigValue(key, envValue, "environment variable (check /devops/k8s/<env>/<manifest.yaml>)", logger, sensitiveSetting);
            return envValue;
        }

        var confValue = configuration[key];
        if (!String.IsNullOrEmpty(confValue))
        {
            LogConfigValue(key, confValue, "appsettings.<env>.json", logger, sensitiveSetting);
            return confValue;
        }

        if (required)
        {
            throw new Exception($"value for required {key} is null or empty!");
        }
        else
        {
            logger.LogInformation("Ignoring non-required config value {key}", key);
            return String.Empty;
        }

    }
    private static void LogConfigValue(string key, string value, string source, ILogger logger, bool sensitiveSetting = false)
    {

        if (sensitiveSetting)
        {
            logger.LogInformation($"setting {key} to <redacted> from {source}");
        }
        else
        {
            logger.LogInformation($"setting {key} to {value} from {source}");
        }
    }
}
