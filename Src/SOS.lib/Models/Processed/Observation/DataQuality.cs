using Nest;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Class related to data quality
    /// </summary>
    public class DataQuality
    {
        /// <summary>
        /// Hashed key created from observation date + taxon id + position
        /// </summary>
        [Keyword]
        public string UniqueKey { get; set; }
    }
}
