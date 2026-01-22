using SOS.Lib.Configuration.Shared;
using SOS.Lib.Helpers;
using SOS.Status.Web.Client.Models;
using System.Text.Json;

public static class Settings
{
    public static string BlazorRenderMode { get; private set; } = "Server"; // "WebAssembly", "Server" or "Auto"
    public static UserServiceConfiguration UserServiceConfiguration { get; set; } = new();
    public static HttpClientsConfiguration HttpClientsConfiguration { get; set; } = new();
    public static MongoDbConfiguration ProcessDbConfiguration { get; set; } = new();
    public static ElasticSearchConfiguration SearchDbConfiguration { get; set; } = new();
    public static string RedisConnectionString { get; set; } = "";
    public static string ClientSecret { get; set; } = "";
    public static string SearchDbUserName { get; set; } = "";
    public static string SearchDbPassword { get; set; } = "";
    public static string ProcessDbUserName { get; set; } = "";
    public static string ProcessDbPassword { get; set; } = "";

    // Prioritizes environment variables but falls back on appsettings.<env>.json and throws if neither are found
    // Logs the source of the setting (environment variable or appsettings.<env>.json) and the setting but redacts sensitive information
    public static void Init(IConfiguration configuration)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Debug).AddConsole());
        var logger = loggerFactory.CreateLogger("Settings");

        BlazorRenderMode = GetConfigValueString("BlazorRenderMode", configuration, logger, sensitiveSetting: false, required: false);
        HttpClientsConfiguration = GetConfigSection<HttpClientsConfiguration>("HttpClients", configuration, logger, sensitiveSetting: false);
        
        var observationApiUrl = NetAspireHelper.GetServiceEndpoint("sos-observations-api", "http"); // Get Observation API url from .Net Aspire configuration.
        if (!string.IsNullOrEmpty(observationApiUrl))
        {
            HttpClientsConfiguration.SosObservationsApi.BaseAddress = observationApiUrl;            
        }
        var analysisApiUrl = NetAspireHelper.GetServiceEndpoint("sos-analysis-api", "http"); // Get Analysis API url from .Net Aspire configuration.
        if (!string.IsNullOrEmpty(analysisApiUrl))
        {
            HttpClientsConfiguration.SosAnalysisApi.BaseAddress = analysisApiUrl;
        }
        var adminApiUrl = NetAspireHelper.GetServiceEndpoint("sos-administration-api", "http"); // Get Admin API url from .Net Aspire configuration.
        if (!string.IsNullOrEmpty(adminApiUrl))
        {
            HttpClientsConfiguration.SosAdministrationApi.BaseAddress = adminApiUrl;
        }

        // User service config
        UserServiceConfiguration = GetConfigSection<UserServiceConfiguration>("UserServiceConfiguration", configuration, logger, sensitiveSetting: false);
        ClientSecret = GetConfigValueString("ClientSecret", configuration, logger, sensitiveSetting: true, required: false);
        if (UserServiceConfiguration.ClientSecret == "SECRET_PLACEHOLDER")
        {
            UserServiceConfiguration.ClientSecret = ClientSecret;
            logger.LogInformation("replaced SECRET_PLACEHOLDER in UserServiceConfiguration.ClientSecret with the value in ClientSecret");
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

        RedisConnectionString = GetConfigValueString("RedisConnectionString", configuration, logger, sensitiveSetting: true);
    }

    private static T GetConfigSection<T>(string key, IConfiguration configuration, ILogger logger, bool sensitiveSetting = false, bool required = true) where T : class
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
            return null!;
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
