namespace SOS.Import.Entities.Artportalen
{
    public class AreaEntity
    {
        /// <summary>
        ///     Type of area
        /// </summary>
        public int AreaDatasetId { get; set; }

        /// <summary>
        ///     Area Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Area geometry
        /// </summary>
        public string PolygonWKT { get; set; }

        /// <summary>
        ///     Name of area
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Feature Id.
        /// </summary>
        public string FeatureId { get; set; }

        /// <summary>
        ///     Parent Id.
        /// </summary>
        public int? ParentId { get; set; }
    }
}