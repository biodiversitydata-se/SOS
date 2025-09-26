using SOS.Status.Web.Client.Abstractions;

namespace SOS.Status.Web.Client.Models;

public class AppEnvironment : IAppEnvironment
{
    public string EnvironmentName { get; }

    public AppEnvironment(string environmentName)
    {
        EnvironmentName = environmentName ?? "Production";
    }

    public bool IsLocalOrDev()
    {
        var env = EnvironmentName.ToLowerInvariant();
        return env == "local" || env == "dev";
    }
}