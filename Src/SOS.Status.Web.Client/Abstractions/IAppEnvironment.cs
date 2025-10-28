namespace SOS.Status.Web.Client.Abstractions;

public interface IAppEnvironment
{
    string EnvironmentName { get; }
    bool IsLocalOrDev();
    bool IsProduction();
}