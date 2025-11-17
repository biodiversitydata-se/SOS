using SOS.Shared.Api.Dtos.Enum;

namespace SOS.Shared.Api.Dtos;

/// <summary>
/// User role authority area.
/// </summary>
public class UserAreaDto
{
    /// <summary>
    /// Area buffer in meters
    /// </summary>
    public int? Buffer { get; set; }

    /// <summary>
    /// Area type.
    /// </summary>
    public AreaTypeDto AreaType { get; set; }

    /// <summary>
    /// Area FeatureId.
    /// </summary>
    public string FeatureId { get; set; }

    /// <summary>
    /// Area name.
    /// </summary>
    public string Name { get; set; }
}
