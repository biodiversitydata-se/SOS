namespace SOS.Batch.Import.AP.Models.Aggregates
{
    /// <summary>
    /// Site object
    /// </summary>
    public class SiteAggregate
    {
        /// <summary>
        /// County of site
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// Id of site
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of site
        /// </summary>
        public string Municipality { get; set; }

        /// <summary>
        /// Name of site
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// X coordinate of site
        /// </summary>
        public int XCoord { get; set; }

        /// <summary>
        /// Y coordinate of site
        /// </summary>
        public int YCoord { get; set; }
    }
}
