namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Search filter for public observation endpoints.
/// </summary>
public class SearchFilterDto : SearchFilterBaseDto
{
    /// <summary>Selects which observation fields to return. When omitted, the Minimum field set is used.</summary>
    public OutputFilterDto? Output { get; set; }
}
