namespace SOS.Lib.Enums
{
    /// <summary>
    /// Enumeration of predefined coordinate system.
    /// This enumeration makes it easier to handle commonly
    /// used coordinate systems.
    /// Definitions of WKT's was retrieved from
    /// http://spatialreference.org/
    /// </summary>
    public enum CoordinateSys
    {
        /// <summary>
        /// Google mercator
        /// </summary>
        WebMercator = 3857,

        /// <summary>
        /// RT90
        /// </summary>
        Rt90_25_gon_v = 2400,

        /// <summary>
        /// SWEREF 99
        /// </summary>
        SWEREF99 = 3021,

        /// <summary>
        /// SWEREF 99 TM
        /// </summary>
        SWEREF99_TM = 3006,


        /// <summary>
        /// WGS 84
        /// </summary>
        WGS84 = 4326
    }
}
