using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Configuration.Shared;
using SOS.Observations.Api.Configuration;
using SOS.Shared.Api.Configuration;
using System;
using System.Text.Json;

public static class Settings
{
    public static bool CorsAllowAny { get; set; } = false;
    public static UserServiceConfiguration UserServiceConfiguration { get; set; } = new();
    public static ObservationApiConfiguration ObservationApiConfiguration { get; set; } = new();
    public static VocabularyConfiguration VocabularyConfiguration { get; set; } = new();
    public static ArtportalenApiServiceConfiguration ArtportalenApiServiceConfiguration { get; set; } = new();
    public static SemaphoreLimitsConfiguration SemaphoreLimitsConfiguration { get; set; } = new();
    public static AreaConfiguration AreaConfiguration { get; set; } = new();
    public static InputValaidationConfiguration InputValidationConfiguration { get; set; } = new();
    public static ApplicationInsights ApplicationInsightsConfiguration { get; set; } = new();
    public static BlobStorageConfiguration BlobStorageConfiguration { get; set; } = new();
    public static CryptoConfiguration CryptoConfiguration { get; set; } = new();
    public static HangfireDbConfiguration HangfireDbConfiguration { get; set; } = new();
    public static MongoDbConfiguration ProcessDbConfiguration { get; set; } = new();
    public static ElasticSearchConfiguration SearchDbConfiguration { get; set; } = new();
    public static DevOpsConfiguration DevOpsConfiguration { get; set; } = new();
    public static HealthCheckConfiguration HealthCheckConfiguration { get; set; } = new();
    public static string ClientSecret { get; set; } = "";
    public static string InstrumentationKey { get; set; } = "";
    public static string BlobStorageConnectionString { get; set; } = "";
    public static string BlobStorageKey { get; set; } = "";
    public static string Password { get; set; } = "";
    public static string Salt { get; set; } = "";
    public static string AzureSubscriptionKey { get; set; } = "";
    public static string DataCiteUserName { get; set; } = "";
    public static string DataCitePassword { get; set; } = "";
    public static string SearchDbUserName { get; set; } = "";
    public static string SearchDbPassword { get; set; } = "";
    public static string ProcessDbUserName { get; set; } = "";
    public static string ProcessDbPassword { get; set; } = "";
    public static string HangfireDbUserName { get; set; } = "";
    public static string HangfireDbPassword { get; set; } = "";

    // Prioritizes environment variables but falls back on appsettings.<env>.json and throws if neither are found
    // Logs the source of the setting (environment variable or appsettings.<env>.json) and the setting but redacts sensitive information
    public static void Init(IConfiguration configuration)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Debug).AddConsole());
        var logger = loggerFactory.CreateLogger("Settings");

        CorsAllowAny = GetConfigValueString("CorsAllowAny", configuration, logger, sensitiveSetting: false, required: false)?.Equals("true", StringComparison.CurrentCultureIgnoreCase) ?? false;
        ObservationApiConfiguration = GetConfigSection<ObservationApiConfiguration>("ObservationApiConfiguration", configuration, logger);
        VocabularyConfiguration = GetConfigSection<VocabularyConfiguration>("VocabularyConfiguration", configuration, logger);
        SemaphoreLimitsConfiguration = GetConfigSection<SemaphoreLimitsConfiguration>("SemaphoreLimitsConfiguration", configuration, logger);        
        ArtportalenApiServiceConfiguration = GetConfigSection<ArtportalenApiServiceConfiguration>("ArtportalenApiServiceConfiguration", configuration, logger);
        AreaConfiguration = GetConfigSection<AreaConfiguration>("AreaConfiguration", configuration, logger);
        InputValidationConfiguration = GetConfigSection<InputValaidationConfiguration>("InputValaidationConfiguration", configuration, logger);
        DevOpsConfiguration = GetConfigSection<DevOpsConfiguration>("DevOpsConfiguration", configuration, logger);

        // User service config
        UserServiceConfiguration = GetConfigSection<UserServiceConfiguration>("UserServiceConfiguration", configuration, logger, sensitiveSetting: false);
        ClientSecret = GetConfigValueString("ClientSecret", configuration, logger, sensitiveSetting: true, required: false);
        if (UserServiceConfiguration.ClientSecret == "SECRET_PLACEHOLDER")
        {
            UserServiceConfiguration.ClientSecret = ClientSecret;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in UserServiceConfiguration.ClientSecret with the value in ClientSecret");
        }

        // Health check config
        HealthCheckConfiguration = GetConfigSection<HealthCheckConfiguration>("HealthCheckConfiguration", configuration, logger, sensitiveSetting: false);
        AzureSubscriptionKey = GetConfigValueString("AzureSubscriptionKey", configuration, logger, sensitiveSetting: true, required: false);
        if (HealthCheckConfiguration.AzureSubscriptionKey == "SECRET_PLACEHOLDER")
        {
            HealthCheckConfiguration.AzureSubscriptionKey = AzureSubscriptionKey;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in HealthCheckConfiguration.AzureSubscriptionKey with the value in AzureSubscriptionKey");
        }

        // ApplicationInsights
        ApplicationInsightsConfiguration = GetConfigSection<ApplicationInsights>("ApplicationInsights", configuration, logger, sensitiveSetting: false, required: false);
        if (ApplicationInsightsConfiguration != null)
        {
            InstrumentationKey = GetConfigValueString("InstrumentationKey", configuration, logger, sensitiveSetting: true, required: false);
            if (ApplicationInsightsConfiguration.InstrumentationKey.Contains("SECRET_PLACEHOLDER"))
            {
                ApplicationInsightsConfiguration.InstrumentationKey = InstrumentationKey;
                logger.LogInformation("replaced SECRET_PLACEHOLDER in ApplicationInsightsConfiguration.InstrumentationKey with the value in InstrumentationKey");
            }
        }

        // Blob storage
        BlobStorageConfiguration = GetConfigSection<BlobStorageConfiguration>("BlobStorageConfiguration", configuration, logger, sensitiveSetting: false);
        BlobStorageConnectionString = GetConfigValueString("BlobStorageConnectionString", configuration, logger, sensitiveSetting: true, required: false);
        if (BlobStorageConfiguration.ConnectionString.Contains("SECRET_PLACEHOLDER"))
        {
            BlobStorageConfiguration.ConnectionString = BlobStorageConnectionString;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in BlobStorageConfiguration.ConnectionString with the value in BlobStorageConnectionString");
        }
        BlobStorageKey = GetConfigValueString("BlobStorageKey", configuration, logger, sensitiveSetting: true, required: false);
        if (BlobStorageConfiguration.Key.Contains("SECRET_PLACEHOLDER"))
        {
            BlobStorageConfiguration.Key = BlobStorageKey;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in BlobStorageConfiguration.Key with the value in BlobStorageKey");
        }

        // CryptoConfiguration
        CryptoConfiguration = GetConfigSection<CryptoConfiguration>("CryptoConfiguration", configuration, logger);
        Password = GetConfigValueString("Password", configuration, logger, sensitiveSetting: true, required: false);
        if (CryptoConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            CryptoConfiguration.Password = Password;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in CryptoConfiguration.Password with the value in Password");
        }
        Salt = GetConfigValueString("Salt", configuration, logger, sensitiveSetting: true, required: false);
        if (CryptoConfiguration.Salt.Contains("SECRET_PLACEHOLDER"))
        {
            CryptoConfiguration.Salt = Salt;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in CryptoConfiguration.Salt with the value in Salt");
        }

        // Hangfire db
        HangfireDbConfiguration = GetConfigSection<HangfireDbConfiguration>("HangfireDbConfiguration", configuration, logger, sensitiveSetting: false);
        HangfireDbUserName = GetConfigValueString("HangfireDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (HangfireDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            HangfireDbConfiguration.UserName = HangfireDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in HangfireDbConfiguration.UserName with the value in HangfireDbUserName");
        }
        HangfireDbPassword = GetConfigValueString("HangfireDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (HangfireDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            HangfireDbConfiguration.Password = HangfireDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in HangfireDbConfiguration.Password with the value in HangfireDbPassword");
        }

        // Process db
        ProcessDbConfiguration = GetConfigSection<MongoDbConfiguration>("ProcessDbConfiguration", configuration, logger, sensitiveSetting: false);
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

        // Elastic search db
        SearchDbConfiguration = GetConfigSection<ElasticSearchConfiguration>("SearchDbConfiguration", configuration, logger);
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
            return default;
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
