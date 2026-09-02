namespace SOS.Api.Contracts.Observations.Requests;
/// <summary>
/// Sensitive observations generalization filter.
/// </summary>
public enum SensitiveGeneralizationFilterDto
{

    DontIncludeGeneralizedObservations = 0,

    IncludeGeneralizedObservations = 1,

    OnlyGeneralizedObservations = 2,

}


