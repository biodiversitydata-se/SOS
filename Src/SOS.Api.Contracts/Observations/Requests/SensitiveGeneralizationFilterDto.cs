namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Sensitive observations generalization filter.
/// </summary>
public enum SensitiveGeneralizationFilterDto
{
    /// <summary>
    /// Exclude sensitive observations that also have a generalized public observation.
    /// </summary>
    DontIncludeGeneralizedObservations = 0,
    /// <summary>
    /// Include sensitive observations that also have a generalized public observation.
    /// </summary>
    IncludeGeneralizedObservations = 1,
    /// <summary>
    /// Include only sensitive observations that also have a generalized public observation.
    /// </summary>
    OnlyGeneralizedObservations = 2,
}


