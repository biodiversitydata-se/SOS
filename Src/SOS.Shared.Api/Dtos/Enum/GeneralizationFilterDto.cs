namespace SOS.Shared.Api.Dtos.Enum
{
    /// <summary>
    /// Generalization filter.
    /// </summary>
    public class GeneralizationFilterDto
    {
        /// <summary>
        /// Sensitive observations generalizations filter.
        /// </summary>
        public SensitiveGeneralizationFilterDto? SensitiveGeneralizationFilter { get; set; }

        /// <summary>
        /// Public observations generalizations filter.
        /// </summary>
        public PublicGeneralizationFilterDto? PublicGeneralizationFilter { get; set; }
    }
}