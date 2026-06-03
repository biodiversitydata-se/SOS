using SOS.Lib.Dtos.Enum;

namespace SOS.Lib.Dtos.Filter;

/// <summary>
/// Area filter.
/// </summary>
public class AreaFilterDto
{
    /// <summary>
    ///     Type of area
    /// </summary>
    public AreaTypeDto AreaType { get; set; }

    /// <summary>
    ///    Feature
    /// </summary>
    public string FeatureId { get; set; } = string.Empty;
}