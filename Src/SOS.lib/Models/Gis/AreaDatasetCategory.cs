namespace SOS.Lib.Models.Gis
{
    /// <summary>
    /// AreaDataset category.
    /// </summary>
    public enum AreaDatasetCategory
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Political boundary.
        /// </summary>
        PoliticalBoundary = 1,

        /// <summary>
        /// Interest area.
        /// </summary>
        InterestArea = 2,

        /// <summary>
        /// Restricted area.
        /// </summary>
        RestrictedArea = 3,

        /// <summary>
        /// Atlas.
        /// </summary>
        Atlas = 4,

        /// <summary>
        /// Validate regions.
        /// </summary>
        ValidateRegions = 5,

        /// <summary>
        /// Other.
        /// </summary>
        Other = 50
    }
}