using SOS.Lib.Dtos.Enum;

namespace SOS.Lib.Dtos.Filter;

/// <summary>
/// Internal search filter.
/// </summary>
public class SearchFilterInternalBaseDto : SearchFilterBaseDto
{
    /// <summary>
    /// Artportalen specific search properties
    /// </summary>
    public ExtendedFilterDto? ExtendedFilter{ get; set; }

    /// <summary>
    /// Generalization filter.
    /// </summary>
    public GeneralizationFilterDto? GeneralizationFilter { get; set; }        

    /// <summary>
    /// Observation protection filter
    /// </summary>
    public ProtectionFilterDto? ProtectionFilter { get; set; }
}