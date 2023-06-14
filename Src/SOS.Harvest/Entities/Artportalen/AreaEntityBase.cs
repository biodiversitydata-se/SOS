namespace SOS.Harvest.Entities.Artportalen
{
    public class AreaEntityBase
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public int AreaDatasetId { get; set; }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Feature Id.
        /// </summary>
        public string? FeatureId { get; set; }
    }
}