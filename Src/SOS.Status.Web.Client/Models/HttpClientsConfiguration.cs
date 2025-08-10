namespace SOS.Status.Web.Client.Models;

public class HttpClientsConfiguration
{
    public ApiClientConfiguration SosObservationsApi { get; set; } = new();
    public ApiClientConfiguration SosAdministrationApi { get; set; } = new();
    public ApiClientConfiguration SosAnalysisApi { get; set; } = new();
    public ApiClientConfiguration SosElasticsearchProxy { get; set; } = new();
    public ApiClientConfiguration SosDataStewardshipApi { get; set; } = new();
}

public class ApiClientConfiguration
{
    public string BaseAddress { get; set; } = string.Empty;
}