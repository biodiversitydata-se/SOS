namespace SOS.Harvest.Entities.Artportalen
{
    public class AreaEntity : AreaEntityBase
    {
        /// <summary>
        ///     Area geometry
        /// </summary>
        public string PolygonWKT { get; set; }

        /// <summary>
        ///     Parent Id.
        /// </summary>
        public int? ParentId { get; set; }
    }
}