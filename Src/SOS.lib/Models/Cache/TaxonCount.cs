namespace SOS.Lib.Models.Cache
{
    /// <summary>
    /// Cached taxon count.
    /// </summary>
    public class TaxonCount
    {
        /// <summary>
        /// Observation count.
        /// </summary>
        public int ObservationCount { get; set; }

        /// <summary>
        /// Province count.
        /// </summary>
        public int ProvinceCount { get; set; }
    }
}