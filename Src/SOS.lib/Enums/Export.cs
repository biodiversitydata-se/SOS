namespace SOS.Lib.Enums
{
    /// <summary>
    /// Supported export formats 
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>
        /// Export format DarwinCore Archive
        /// </summary>
        DwC,
        /// <summary>
        /// Export format GeoJson
        /// </summary>
        GeoJson,
        /// <summary>
        /// Export format Excel
        /// </summary>
        Excel
    }

    /// <summary>
    /// Export property sets
    /// </summary>
    public enum ExportPropertySet
    {
        /// <summary>
        /// Minimum of properties exported
        /// </summary>
        Minimum,
        /// <summary>
        /// A extended set of properties exported
        /// </summary>
        Extended,
        /// <summary>
        /// All properties exported
        /// </summary>
        All
    }
}
