namespace SOS.Shared.Api.Dtos.Enum
{
    /// <summary>
    /// Sensitive observations generalization filter.
    /// </summary>
    public enum SensitiveGeneralizationFilterDto
    {
        /// <summary>
        /// Dont include sensitive observations that also has generalized public observation.
        /// </summary>
        DontIncludeGeneralizedObservations = 0,

        /// <summary>
        /// Include sensitive observations that also has generalized public observation.
        /// </summary>
        IncludeGeneralizedObservations = 1,

        /// <summary>
        /// Only include sensitive observations that also has generalized public observation.
        /// </summary>
        OnlyGeneralizedObservations = 2
    }
}