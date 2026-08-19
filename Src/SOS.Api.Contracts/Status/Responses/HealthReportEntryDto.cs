namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Health status for one registered health check.</summary>
public class HealthReportEntryDto
{
    public string? Status { get; set; }

    public string? Description { get; set; }

    public string? Duration { get; set; }

    public List<string>? Tags { get; set; }

    public Dictionary<string, object>? Data { get; set; }
}


