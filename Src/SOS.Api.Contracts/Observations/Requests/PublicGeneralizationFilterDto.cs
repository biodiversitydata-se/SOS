namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Public observations generalization filter.
/// </summary>
public enum PublicGeneralizationFilterDto
{
    /// <summary>Include both generalized and non-generalized observations.</summary>
    NoFilter = 0,
    /// <summary>Include only generalized observations.</summary>
    OnlyGeneralized = 1,
    /// <summary>Exclude generalized observations.</summary>
    DontIncludeGeneralized = 2,
}


