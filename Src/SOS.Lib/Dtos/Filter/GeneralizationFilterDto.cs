using SOS.Lib.Dtos.Enum;

namespace SOS.Lib.Dtos.Filter;

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