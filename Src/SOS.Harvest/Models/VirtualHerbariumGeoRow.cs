namespace SOS.Harvest.Models;

/// <summary>
///     Per-observation geographic coordinate resolution row for Excel export.
/// </summary>
public class VirtualHerbariumGeoRow
{
    /// <summary>
    ///     Province as it appears in the verbatim data.
    /// </summary>
    public string? VerbatimProvince { get; set; }

    /// <summary>
    ///     District (socken/distrikt) as it appears in the verbatim data.
    /// </summary>
    public string? VerbatimDistrict { get; set; }

    /// <summary>
    ///     Location as it appears in the verbatim data.
    /// </summary>
    public string? VerbatimLocation { get; set; }

    /// <summary>
    ///     How coordinates were resolved, e.g. "Verbatim coordinates", "District=>Community", "No match".
    /// </summary>
    public string Match { get; set; } = string.Empty;

    /// <summary>
    ///     Coordinate uncertainty in meters after area lookup (null if no coordinates were resolved).
    /// </summary>
    public int? CoordinateUncertaintyInMeters { get; set; }

    /// <summary>
    ///     Province name resolved from coordinates.
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    ///     Municipality name resolved from coordinates.
    /// </summary>
    public string? Municipality { get; set; }

    /// <summary>
    ///     Parish name resolved from coordinates.
    /// </summary>
    public string? Parish { get; set; }
}
