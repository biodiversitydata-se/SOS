namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Search filter for public observation endpoints.
/// </summary>
public class SearchFilterDto : SearchFilterBaseDto
{
    public OutputFilterDto? Output { get; set; }
}
