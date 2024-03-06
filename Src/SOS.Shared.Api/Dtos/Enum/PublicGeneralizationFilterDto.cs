namespace SOS.Shared.Api.Dtos.Enum
{
    /// <summary>
    /// Public observations generalization filter.
    /// </summary>
    public enum PublicGeneralizationFilterDto
    {
        /// <summary>
        /// Include both generalized and not generalized observations.
        /// </summary>
        NoFilter = 0,

        /// <summary>
        /// Only generalized observations.
        /// </summary>
        OnlyGeneralized = 1,

        /// <summary>
        /// Don't include generalized observations.
        /// </summary>
        DontIncludeGeneralized = 2
    }
}