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

    public static ImportConfiguration ImportConfiguration { get; set; } = new();
    public static ProcessConfiguration ProcessConfiguration { get; set; } = new();
    public static UserServiceConfiguration UserServiceConfiguration { get; set; } = new();
    public static SosApiConfiguration SosApiConfiguration { get; set; } = new();
    public static ApplicationInsightsConfiguration ApplicationInsightsConfiguration { get; set; } = new();
    public static MongoDbConfiguration VerbatimDbConfiguration { get; set; } = new();
    public static MongoDbConfiguration ProcessDbConfiguration { get; set; } = new();
    public static HangfireDbConfiguration HangfireDbConfiguration { get; set; } = new();
    public static string InstrumentationKey { get; set; } = "";
    public static string VerbatimDbUserName { get; set; } = "";
    public static string VerbatimDbPassword { get; set; } = "";
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

        AllowedOrigins = GetConfigValueString("AllowedOrigins", configuration, logger).Split(",").Select(s => s.Trim()).ToArray();

        ImportConfiguration = GetConfigSection<ImportConfiguration>("ImportConfiguration", configuration, logger);
        ProcessConfiguration = GetConfigSection<ProcessConfiguration>("ProcessConfiguration", configuration, logger);
        UserServiceConfiguration = GetConfigSection<UserServiceConfiguration>("UserServiceConfiguration", configuration, logger);
        SosApiConfiguration = GetConfigSection<SosApiConfiguration>("SosApiConfiguration", configuration, logger);

        // ApplicationInsights
        ApplicationInsightsConfiguration = GetConfigSection<ApplicationInsightsConfiguration>("ApplicationInsights", configuration, logger);
        InstrumentationKey = GetConfigValueString("InstrumentationKey", configuration, logger, sensitiveSetting: true, required: false);
        if (ApplicationInsightsConfiguration.InstrumentationKey.Contains("SECRET_PLACEHOLDER"))
        {
            ApplicationInsightsConfiguration.InstrumentationKey = InstrumentationKey;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in ApplicationInsightsConfiguration.InstrumentationKey with the value in InstrumentationKey");
        }

        // Verbatim db
        VerbatimDbConfiguration = GetConfigSection<MongoDbConfiguration>("VerbatimDbConfiguration", configuration, logger);
        VerbatimDbUserName = GetConfigValueString("VerbatimDbUserName", configuration, logger, sensitiveSetting: true, required: false);
        if (VerbatimDbConfiguration.UserName.Contains("SECRET_PLACEHOLDER"))
        {
            VerbatimDbConfiguration.UserName = VerbatimDbUserName;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in VerbatimDbConfiguration.UserName with the value in VerbatimDbUserName");
        }
        VerbatimDbPassword = GetConfigValueString("VerbatimDbPassword", configuration, logger, sensitiveSetting: true, required: false);
        if (VerbatimDbConfiguration.Password.Contains("SECRET_PLACEHOLDER"))
        {
            VerbatimDbConfiguration.Password = VerbatimDbPassword;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in VerbatimDbConfiguration.Password with the value in VerbatimDbPassword");
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
