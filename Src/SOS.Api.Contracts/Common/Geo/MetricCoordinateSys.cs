namespace SOS.Api.Contracts.Common.Geo;
/// <summary>Coordinate reference system used for metric aggregation.</summary>
public enum MetricCoordinateSys
{

    /// <summary>SWEREF 99 TM (EPSG:3006).</summary>
    SWEREF99_TM = 3006,

    /// <summary>Deprecated alias for <see cref="ETRS89_LAEA_Europe"/>.</summary>
    [Obsolete("ETRS89_LAEA_Europe should be used instead of this.")]
    ETRS89 = 3035,

    /// <summary>ETRS89 Lambert azimuthal equal-area for Europe (EPSG:3035).</summary>
    ETRS89_LAEA_Europe = 3035
}
