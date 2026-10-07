namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Filters observations by whether the organism occurs naturally at the location.</summary>
public enum SightingUnspontaneousFilterDto
{

    /// <summary>Include both natural and non-natural occurrences.</summary>
    NoFilter = 0,

    /// <summary>Only include natural occurrences.</summary>
    NotUnspontaneous = 1,

    /// <summary>Only include non-natural occurrences.</summary>
    Unspontaneous = 2,

}


