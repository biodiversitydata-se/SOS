namespace SOS.Lib.Configuration.Shared;

/// <summary>
/// Configuration for application insights
/// </summary>
public class ApplicationInsights
{
    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString => $"InstrumentationKey={InstrumentationKey};IngestionEndpoint=https://northeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://northeurope.livediagnostics.monitor.azure.com/;ApplicationId=e2ecaeec-e9eb-4ade-ab9a-2cabbe7b26c5";

    /// <summary>
    /// Log request body 
    /// </summary>
    public bool EnableRequestBodyLogging { get; set; }

    /// <summary>
    /// Log search response document count
    /// </summary>
    public bool EnableSearchResponseCountLogging { get; set; }

    /// <summary>
    /// Instrumentation key
    /// </summary>
    public string InstrumentationKey { get; set; }
}