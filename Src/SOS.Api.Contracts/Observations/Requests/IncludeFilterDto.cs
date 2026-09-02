namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Limits a search to explicitly listed occurrence identifiers.
/// </summary>
public class IncludeFilterDto
{
    /// <summary>
    /// Occurrence identifiers to include.
    /// </summary>
    public ICollection<string>? OccurrenceIds { get; set; }
}
