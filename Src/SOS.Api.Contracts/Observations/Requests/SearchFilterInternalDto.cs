namespace SOS.Api.Contracts.Observations.Requests;

/// <summary>
/// Filter for internal SOS observation search endpoints.
/// </summary>
public class SearchFilterInternalDto : SearchFilterInternalBaseDto
{
    /// <summary>
    /// Controls whether the exact hit count is returned when the search matches more than 10,000 observations.
    /// </summary>
    public bool? IncludeRealCount { get; set; }

    /// <summary>
    /// The fields and extended data included in each observation response.
    /// </summary>
    public OutputFilterExtendedDto? Output { get; set; }
}
