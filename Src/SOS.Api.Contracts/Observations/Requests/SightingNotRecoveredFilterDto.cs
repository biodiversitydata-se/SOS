namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Filters observations marked as not found again at the location.</summary>
public enum SightingNotRecoveredFilterDto
{

    /// <summary>Do not filter by whether the species was found again.</summary>
    NoFilter = 0,

    /// <summary>Only include observations marked as not found again.</summary>
    OnlyNotRecovered = 1,

    /// <summary>Exclude observations marked as not found again.</summary>
    DontIncludeNotRecovered = 2,

    /// <summary>Include observations whether or not the species was found again.</summary>
    IncludeNotRecovered = 3,

}


