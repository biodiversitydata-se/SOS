namespace SOS.Lib.Enums
{
    /// <summary>
    ///     Enumeration of predefined coordinate system.
    ///     This enumeration makes it easier to handle commonly
    ///     used coordinate systems.
    ///     Definitions of WKT's was retrieved from
    ///     http://spatialreference.org/
    /// </summary>
    public enum CoordinateSys
    {
        /// <summary>
        ///     WGS84 Web Mercator
        /// </summary>
        /// <remarks>The same as GoogleMercator</remarks>
        WebMercator = 3857,

        /// <summary>
        ///     RT90 2.5 gon V
        /// </summary>
        /// <remarks>The same as RT90 2.5 gon W</remarks>
        Rt90_25_gon_v = 3021,

        /// <summary>
        ///     SWEREF 99
        /// </summary>
        SWEREF99 = 4619,

        /// <summary>
        ///     SWEREF 99 TM
        /// </summary>
        SWEREF99_TM = 3006,

        /// <summary>
        ///     WGS 84
        /// </summary>
        WGS84 = 4326,

        /// <summary>
        ///     ETRS-LAEA
        /// </summary>
        ETRS89 = 3035
    }
}