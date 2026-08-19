namespace SOS.Api.Contracts.Analysis.Requests;

/// <summary>
/// Coordinate reference system used for an analysis result.
/// </summary>
public enum AnalysisCoordinateSystem
{
    /// <summary>Web Mercator (EPSG:3857).</summary>
    WebMercator = 3857,
    /// <summary>RT90 2.5 gon west (EPSG:3021).</summary>
    Rt90_25_gon_v = 3021,
    /// <summary>SWEREF 99 geographic coordinates (EPSG:4619).</summary>
    SWEREF99 = 4619,
    /// <summary>SWEREF 99 TM (EPSG:3006).</summary>
    SWEREF99_TM = 3006,
    /// <summary>WGS 84 (EPSG:4326).</summary>
    WGS84 = 4326,
    /// <summary>ETRS89 geographic coordinates (EPSG:4258).</summary>
    ETRS89 = 4258,
    /// <summary>ETRS89 Lambert azimuthal equal-area projection for Europe (EPSG:3035).</summary>
    ETRS89_LAEA_Europe = 3035
}
