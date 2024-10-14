using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SOS.Administration.Gui;
using System.Linq;
using System.Text.Json;

public static class Settings
{
    // For config objects
    public static ApiTestConfiguration ApiTestConfiguration { get; set; } = new();
    public static AuthenticationConfiguration AuthenticationConfiguration { get; set; } = new();
    public static ApplicationInsightsConfiguration ApplicationInsightsConfiguration { get; set; } = new();
    public static MongoDbConfiguration ProcessDbConfiguration { get; set; } = new();
    public static ElasticSearchConfiguration SearchDbConfiguration { get; set; } = new();
    public static TestElasticSearchConfiguration SearchDbTestConfiguration { get; set; } = new();

    // Strings
    public static string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public static string InstrumentationKey { get; set; } = "";
    public static string ApiKey { get; set; } = "";
    public static string ApplicationId { get; set; } = "";
    public static string ProcessDbUserName { get; set; } = "";
    public static string ProcessDbPassword { get; set; } = "";
    public static string SearchDbUserName { get; set; } = "";
    public static string SearchDbPassword { get; set; } = "";
    public static string SearchDbTestUserName { get; set; } = "";
    public static string SearchDbTestPassword { get; set; } = "";
    public static string AuthSecretKey { get; set; } = "";
    public static string AuthSecretPassword { get; set; } = "";

    // Prioritizes environment variables but falls back on appsettings.<env>.json and throws if neither are found
    // Logs the source of the setting (environment variable or appsettings.<env>.json) and the setting but redacts sensitive information
    public static void Init(IConfiguration configuration)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Debug).AddConsole());
        var logger = loggerFactory.CreateLogger("Settings");

        AllowedOrigins = GetConfigValueString("AllowedOrigins", configuration, logger).Split(",").Select(s => s.Trim()).ToArray();
        ApiTestConfiguration = GetConfigSection<ApiTestConfiguration>("ApiTestConfiguration", configuration, logger);

        // These configs get secrets from k8s setup or local secrets.json
        SetupApplicationInsightsConfig(configuration, logger);
        SetupAuthConfig(configuration, logger);
        SetupProcessDbConfig(configuration, logger);
        SetupSearchDbConfig(configuration, logger);
        SetupSearchDbTestConfig(configuration, logger);
    }

    private static void SetupApplicationInsightsConfig(IConfiguration configuration, ILogger logger)
    {
        ApplicationInsightsConfiguration = GetConfigSection<ApplicationInsightsConfiguration>("ApplicationInsightsConfiguration", configuration, logger);
        InstrumentationKey = GetConfigValueString("InstrumentationKey", configuration, logger, sensitiveSetting: true, required: false);
        if (ApplicationInsightsConfiguration.InstrumentationKey.Contains("SECRET_PLACEHOLDER"))
        {
            ApplicationInsightsConfiguration.InstrumentationKey = InstrumentationKey;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ApplicationInsightsConfiguration.InstrumentationKey with the value in InstrumentationKey");
        }
        ApiKey = GetConfigValueString("ApiKey", configuration, logger, sensitiveSetting: true, required: false);
        if (ApplicationInsightsConfiguration.ApiKey.Contains("SECRET_PLACEHOLDER"))
        {
            ApplicationInsightsConfiguration.ApiKey = ApiKey;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ApplicationInsightsConfiguration.ApiKey with the value in ApiKey");
        }
        ApplicationId = GetConfigValueString("ApplicationId", configuration, logger, sensitiveSetting: true, required: false);
        if (ApplicationInsightsConfiguration.ApplicationId.Contains("SECRET_PLACEHOLDER"))
        {
            ApplicationInsightsConfiguration.ApplicationId = ApplicationId;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ApplicationInsightsConfiguration.ApplicationId with the value in ApplicationId");
        }
    }

    private static void SetupAuthConfig(IConfiguration configuration, ILogger logger)
    {
        AuthenticationConfiguration = GetConfigSection<AuthenticationConfiguration>("AuthenticationConfiguration", configuration, logger, sensitiveSetting: true);
        AuthSecretKey = GetConfigValueString("AuthSecretKey", configuration, logger, sensitiveSetting: true, required: false);
        if (AuthenticationConfiguration.SecretKey == "SECRET_PLACEHOLDER")
        {
            AuthenticationConfiguration.SecretKey = AuthSecretKey;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in AuthenticationConfiguration.SecretKey with the value in AuthSecretKey");
        }
        AuthSecretPassword = GetConfigValueString("AuthSecretPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (AuthenticationConfiguration.SecretPassword == "SECRET_PLACEHOLDER")
        {
            AuthenticationConfiguration.SecretPassword = AuthSecretPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in AuthenticationConfiguration.SecretPassword with the value in AuthSecretPassword");
        }
    }

    private static void SetupProcessDbConfig(IConfiguration configuration, ILogger logger)
    {
        ProcessDbConfiguration = GetConfigSection<MongoDbConfiguration>("ProcessDbConfiguration", configuration, logger);
        ProcessDbUserName = GetConfigValueString("ProcessDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (ProcessDbConfiguration.UserName == "SECRET_PLACEHOLDER")
        {
            ProcessDbConfiguration.UserName = ProcessDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ProcessDbConfiguration.UserName with the value in ProcessDbUserName");
        }
        ProcessDbPassword = GetConfigValueString("ProcessDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (ProcessDbConfiguration.Password == "SECRET_PLACEHOLDER")
        {
            ProcessDbConfiguration.Password = ProcessDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ProcessDbConfiguration.Password with the value in ProcessDbPassword");
        }
    }

    private static void SetupSearchDbConfig(IConfiguration configuration, ILogger logger)
    {
        SearchDbConfiguration = GetConfigSection<ElasticSearchConfiguration>("SearchDbConfiguration", configuration, logger);
        SearchDbUserName = GetConfigValueString("SearchDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (SearchDbConfiguration.UserName == "SECRET_PLACEHOLDER")
        {
            SearchDbConfiguration.UserName = SearchDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in SearchDbConfiguration.UserName with the value in SearchDbUserName");
        }
        SearchDbPassword = GetConfigValueString("SearchDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (SearchDbConfiguration.Password == "SECRET_PLACEHOLDER")
        {
            SearchDbConfiguration.Password = SearchDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in SearchDbConfiguration.Password with the value in SearchDbPassword");
        }
    }

    private static void SetupSearchDbTestConfig(IConfiguration configuration, ILogger logger)
    {
        SearchDbTestConfiguration = GetConfigSection<TestElasticSearchConfiguration>("SearchDbTestConfiguration", configuration, logger);
        SearchDbTestUserName = GetConfigValueString("SearchDbTestUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (SearchDbTestConfiguration.UserName == "SECRET_PLACEHOLDER")
        {
            SearchDbTestConfiguration.UserName = SearchDbTestUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in SearchDbConfigurationTest.UserName with the value in SearchDbTestUserName");
        }
        SearchDbTestPassword = GetConfigValueString("SearchDbTestPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (SearchDbTestConfiguration.Password == "SECRET_PLACEHOLDER")
        {
            SearchDbTestConfiguration.Password = SearchDbTestPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in SearchDbConfigurationTest.Password with the value in SearchDbTestPassword");
        }
    }

    private static T GetConfigSection<T>(string key, IConfiguration configuration, ILogger logger, bool sensitiveSetting = false)
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

        throw new Exception($"value for {key} is null or empty!");

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
