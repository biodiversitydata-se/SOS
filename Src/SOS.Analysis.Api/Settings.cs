using SOS.Analysis.Api.Configuration;
using SOS.Lib.Configuration.Shared;
using SOS.Shared.Api.Configuration;
using System.Text.Json;

public static class Settings
{
    public static bool CorsAllowAny { get; set; } = false;
    public static AnalysisConfiguration AnalysisConfiguration { get; set; } = new();
    public static UserServiceConfiguration UserServiceConfiguration { get; set; } = new();
    public static CryptoConfiguration CryptoConfiguration { get; set; } = new();
    public static IdentityServerConfiguration IdentityServer { get; set; } = new();
    public static AreaConfiguration AreaConfiguration { get; set; } = new();
    public static InputValaidationConfiguration InputValaidationConfiguration { get; set; } = new();
    public static ApplicationInsights ApplicationInsights { get; set; } = new();
    public static ElasticSearchConfiguration SearchDbConfiguration { get; set; } = new();
    public static MongoDbConfiguration ProcessDbConfiguration { get; set; } = new();
    public static HangfireDbConfiguration HangfireDbConfiguration { get; set; } = new();
    public static HangfireDbConfiguration LocalHangfireDbConfiguration { get; set; } = new();
    public static string InstrumentationKey { get; set; } = "";
    public static string SearchDbUserName { get; set; } = "";
    public static string SearchDbPassword { get; set; } = "";
    public static string ProcessDbUserName { get; set; } = "";
    public static string ProcessDbPassword { get; set; } = "";
    public static string HangfireDbUserName { get; set; } = "";
    public static string HangfireDbPassword { get; set; } = "";
    public static string Password { get; set; } = "";
    public static string Salt { get; set; } = "";
    public static string ClientSecret { get; set; } = "";

    // Prioritizes environment variables but falls back on appsettings.<env>.json and throws if neither are found
    // Logs the source of the setting (environment variable or appsettings.<env>.json) and the setting but redacts sensitive information
    public static void Init(IConfiguration configuration)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Debug).AddConsole());
        var logger = loggerFactory.CreateLogger("Settings");

        CorsAllowAny = GetConfigValueString("CorsAllowAny", configuration, logger, sensitiveSetting: false, required: false)?.Equals("true", StringComparison.CurrentCultureIgnoreCase) ?? false;
        AnalysisConfiguration = GetConfigSection<AnalysisConfiguration>("AnalysisConfiguration", configuration, logger);

        UserServiceConfiguration = GetConfigSection<UserServiceConfiguration>("UserServiceConfiguration", configuration, logger);
        ClientSecret = GetConfigValueString("ClientSecret", configuration, logger, sensitiveSetting: true, required: false);
        if (UserServiceConfiguration.ClientSecret.Contains("SECRET_PLACEHOLDER"))
        {
            UserServiceConfiguration.ClientSecret = ClientSecret;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in UserServiceConfiguration.ClientSecret with the value in ClientSecret");
        }

        IdentityServer = GetConfigSection<IdentityServerConfiguration>("IdentityServer", configuration, logger);
        AreaConfiguration = GetConfigSection<AreaConfiguration>("AreaConfiguration", configuration, logger);
        InputValaidationConfiguration = GetConfigSection<InputValaidationConfiguration>("InputValaidationConfiguration", configuration, logger);

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

        // ApplicationInsights
        ApplicationInsights = GetConfigSection<ApplicationInsights>("ApplicationInsights", configuration, logger, required: false);
        if (ApplicationInsights != null)
        {
            InstrumentationKey = GetConfigValueString("InstrumentationKey", configuration, logger, sensitiveSetting: true, required: false);
            if (ApplicationInsights.InstrumentationKey.Contains("SECRET_PLACEHOLDER"))
            {
                ApplicationInsights.InstrumentationKey = InstrumentationKey;
                logger.LogInformation("replaced SECRET_PLACEHOLDER in ApplicationInsights.InstrumentationKey with the value in InstrumentationKey");
            }
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

        // Process db
        ProcessDbConfiguration = GetConfigSection<MongoDbConfiguration>("ProcessDbConfiguration", configuration, logger);
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

        // Hangfire db
        HangfireDbConfiguration = GetConfigSection<HangfireDbConfiguration>("HangfireDbConfiguration", configuration, logger);
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
        LocalHangfireDbConfiguration = GetConfigSection<HangfireDbConfiguration>("LocalHangfireDbConfiguration", configuration, logger, sensitiveSetting: false);
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
