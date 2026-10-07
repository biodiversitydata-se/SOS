namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Health status returned by an SOS API.</summary>
public class HealthReportDto
{
    /// <summary>The overall health status of the API.</summary>
    public string? Status { get; set; }

    /// <summary>The total time taken to run the health checks, formatted as a duration string.</summary>
    public string? TotalDuration { get; set; }

    /// <summary>The individual health-check results, keyed by check name.</summary>
    public Dictionary<string, HealthReportEntryDto>? Entries { get; set; }
}


