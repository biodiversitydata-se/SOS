namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Filters observations marked as never found at the location.</summary>
public enum SightingNotPresentFilterDto
{

    /// <summary>Do not filter by whether the species has never been found at the location.</summary>
    NoFilter = 0,

    /// <summary>Exclude observations marked as never found at the location.</summary>
    DontIncludeNotPresent = 1,

    /// <summary>Only include observations marked as never found at the location.</summary>
    OnlyNotPresent = 2,

    /// <summary>Include observations whether or not the species has ever been found at the location.</summary>
    IncludeNotPresent = 3,

}


