namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Health status for one registered health check.</summary>
public class HealthReportEntryDto
{
    /// <summary>The health status reported by this check.</summary>
    public string? Status { get; set; }

    /// <summary>A description of the check result.</summary>
    public string? Description { get; set; }

    /// <summary>The time taken to run this check, formatted as a duration string.</summary>
    public string? Duration { get; set; }

    /// <summary>Tags identifying groups to which this health check belongs.</summary>
    public List<string>? Tags { get; set; }

    /// <summary>Additional diagnostic data reported by the check.</summary>
    public Dictionary<string, object>? Data { get; set; }
}


