namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Summary status for one SOS observation processing run.</summary>
public class ProcessStatusDto
{
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int PublicCount { get; set; }

    public int ProtectedCount { get; set; }

    public int InvalidCount { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }
}


