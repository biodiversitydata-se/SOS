namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>Filters observations by certainty of species identification.</summary>
public enum SightingDeterminationFilterDto
{

    /// <summary>Include observations regardless of identification certainty.</summary>
    NoFilter = 0,

    /// <summary>Exclude observations marked as uncertain identifications.</summary>
    NotUnsureDetermination = 1,

    /// <summary>Only include observations marked as uncertain identifications.</summary>
    OnlyUnsureDetermination = 2,

}


