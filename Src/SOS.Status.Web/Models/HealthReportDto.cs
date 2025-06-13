namespace SOS.Status.Web.Models;

public class HealthReportDto
{
    public string Status { get; set; }
    public string TotalDuration { get; set; }
    public Dictionary<string, HealthReportEntryDto> Entries { get; set; }
}
