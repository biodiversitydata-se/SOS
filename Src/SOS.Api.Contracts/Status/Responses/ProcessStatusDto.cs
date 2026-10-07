namespace SOS.Api.Contracts.Status.Responses;
/// <summary>Summary status for one SOS observation processing run.</summary>
public class ProcessStatusDto
{
    /// <summary>The name of the processing instance.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The status of the processing run.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>The number of public observations processed.</summary>
    public int PublicCount { get; set; }

    /// <summary>The number of protected observations processed.</summary>
    public int ProtectedCount { get; set; }

    /// <summary>The number of observations that failed processing.</summary>
    public int InvalidCount { get; set; }

    /// <summary>The time the processing run started.</summary>
    public DateTime Start { get; set; }

    /// <summary>The time the processing run finished.</summary>
    public DateTime End { get; set; }
}


