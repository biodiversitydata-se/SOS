using SOS.Lib.Enums;

namespace SOS.Harvest.Models.RegionalTaxonSensitiveCategories
{
    internal class Area 
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public AreaType AreaType { get; set; }

        /// <summary>
        ///     Feature Id.
        /// </summary>
        public string? FeatureId { get; set; }
    }
}