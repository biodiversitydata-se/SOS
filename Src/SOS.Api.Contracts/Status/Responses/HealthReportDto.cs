namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Health status returned by an SOS API.</summary>
public class HealthReportDto
{
    public string? Status { get; set; }

    public string? TotalDuration { get; set; }

    public Dictionary<string, HealthReportEntryDto>? Entries { get; set; }
}


