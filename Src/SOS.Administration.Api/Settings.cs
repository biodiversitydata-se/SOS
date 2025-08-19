using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Configuration.Process;
using System;
using System.Linq;
using System.Text.Json;

public static class Settings
{
    public static string[] AllowedOrigins { get; set; } = [];

    public static AuthenticationConfiguration AuthenticationConfiguration { get; set; } = new();
    public static ImportConfiguration ImportConfiguration { get; set; } = new();
    public static ProcessConfiguration ProcessConfiguration { get; set; } = new();
    public static RedisConfiguration RedisConfiguration { get; set; } = new();
    public static UserServiceConfiguration UserServiceConfiguration { get; set; } = new();
    public static SosApiConfiguration SosApiConfiguration { get; set; } = new();
    public static ApplicationInsightsConfiguration ApplicationInsightsConfiguration { get; set; } = new();
    public static MongoDbConfiguration VerbatimDbConfiguration { get; set; } = new();
    public static MongoDbConfiguration ProcessDbConfiguration { get; set; } = new();
    public static HangfireDbConfiguration HangfireDbConfiguration { get; set; } = new();
    public static HangfireDbConfiguration LocalHangfireDbConfiguration { get; set; } = new();
    public static string ClientSecret { get; set; } = "";
    public static string InstrumentationKey { get; set; } = "";

    // Prioritizes environment variables but falls back on appsettings.<env>.json and throws if neither are found
    // Logs the source of the setting (environment variable or appsettings.<env>.json) and the setting but redacts sensitive information
    public static void Init(IConfiguration configuration)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Debug).AddConsole());
        var logger = loggerFactory.CreateLogger("Settings");

        AllowedOrigins = GetConfigValueString("AllowedOrigins", configuration, logger, false, false).Split(",").Select(s => s.Trim()).ToArray();
        AuthenticationConfiguration = GetConfigSection<AuthenticationConfiguration>("AuthenticationConfiguration", configuration, logger, sensitiveSetting: false);
        ImportConfiguration = GetConfigSection<ImportConfiguration>("ImportConfiguration", configuration, logger, sensitiveSetting: false);
        ProcessConfiguration = GetConfigSection<ProcessConfiguration>("ProcessConfiguration", configuration, logger, sensitiveSetting: false);
        SosApiConfiguration = GetConfigSection<SosApiConfiguration>("SosApiConfiguration", configuration, logger, sensitiveSetting: false);

        UserServiceConfiguration = GetConfigSection<UserServiceConfiguration>("UserServiceConfiguration", configuration, logger, sensitiveSetting: false);
        if (UserServiceConfiguration != null){
            ClientSecret = GetConfigValueString("ClientSecret", configuration, logger, sensitiveSetting: true, required: false);
            if (UserServiceConfiguration.ClientSecret == "SECRET_PLACEHOLDER")
            {
                UserServiceConfiguration.ClientSecret = ClientSecret;
                logger.LogInformation("replaced SECRET_PLACEHOLDER in UserServiceConfiguration.ClientSecret with the value in ClientSecret");
            }
        }

        // ApplicationInsights
        ApplicationInsightsConfiguration = GetConfigSection<ApplicationInsightsConfiguration>("ApplicationInsights", configuration, logger, sensitiveSetting: false, required: false);
        if (ApplicationInsightsConfiguration != null)
        {
            InstrumentationKey = GetConfigValueString("InstrumentationKey", configuration, logger, sensitiveSetting: true, required: false);
            if (ApplicationInsightsConfiguration.InstrumentationKey.Contains("SECRET_PLACEHOLDER"))
            {
                ApplicationInsightsConfiguration.InstrumentationKey = InstrumentationKey;
                logger.LogInformation("replaced SECRET_PLACEHOLDER in ApplicationInsightsConfiguration.InstrumentationKey with the value in InstrumentationKey");
            }
        }

        // Verbatim db
        VerbatimDbConfiguration = GetConfigSection<MongoDbConfiguration>("VerbatimDbConfiguration", configuration, logger, sensitiveSetting: false);
        var verbatimDbUserName = GetConfigValueString("VerbatimDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (VerbatimDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            VerbatimDbConfiguration.UserName = verbatimDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in VerbatimDbConfiguration.UserName with the value in VerbatimDbUserName");
        }
        var verbatimDbPassword = GetConfigValueString("VerbatimDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (VerbatimDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            VerbatimDbConfiguration.Password = verbatimDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in VerbatimDbConfiguration.Password with the value in VerbatimDbPassword");
        }

        // Process db
        ProcessDbConfiguration = GetConfigSection<MongoDbConfiguration>("ProcessDbConfiguration", configuration, logger, sensitiveSetting: false);
        var processDbUserName = GetConfigValueString("ProcessDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (ProcessDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            ProcessDbConfiguration.UserName = processDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ProcessDbConfiguration.UserName with the value in ProcessDbUserName");
        }
        var processDbPassword = GetConfigValueString("ProcessDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (ProcessDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            ProcessDbConfiguration.Password = processDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ProcessDbConfiguration.Password with the value in ProcessDbPassword");
        }
        
        // Hangfire db
        HangfireDbConfiguration = GetConfigSection<HangfireDbConfiguration>("HangfireDbConfiguration", configuration, logger, sensitiveSetting: false);
        var hangfireDbUserName = GetConfigValueString("HangfireDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (HangfireDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            HangfireDbConfiguration.UserName = hangfireDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in HangfireDbConfiguration.UserName with the value in HangfireDbUserName");
        }
        var hangfireDbPassword = GetConfigValueString("HangfireDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (HangfireDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            HangfireDbConfiguration.Password = hangfireDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in HangfireDbConfiguration.Password with the value in HangfireDbPassword");
        }
        LocalHangfireDbConfiguration = GetConfigSection<HangfireDbConfiguration>("LocalHangfireDbConfiguration", configuration, logger, sensitiveSetting: false);

        RedisConfiguration = GetConfigSection<RedisConfiguration>("RedisConfiguration", configuration, logger, sensitiveSetting: false);
        var redisPassword = GetConfigValueString("RedisPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (RedisConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            RedisConfiguration.Password = redisPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in RedisConfiguration.Password with the value in RedisPassword");
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
