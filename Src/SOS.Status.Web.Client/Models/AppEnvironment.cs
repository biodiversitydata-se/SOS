using SOS.Status.Web.Client.Abstractions;

namespace SOS.Status.Web.Client.Models;

public class AppEnvironment : IAppEnvironment
{
    public string EnvironmentName { get; }
    public bool OnlyObservationsAndStatus { get; }

    public AppEnvironment(string environmentName, bool onlyObservationsAndStatus = false)
    {
        EnvironmentName = environmentName ?? "Production";
        OnlyObservationsAndStatus = onlyObservationsAndStatus;
    }

    public bool IsLocalOrDev()
    {
        var env = EnvironmentName.ToLowerInvariant();
        return env == "local" || env == "dev";
    }

    public bool IsProduction()
    {
        var env = EnvironmentName.ToLowerInvariant();
        return env == "prod";
    }
}
