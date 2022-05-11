namespace SOS.Lib.Models.Search
{
    /// <summary>
    ///     Result returned year month aggregation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class YearCountResult
    {
        /// <summary>
        ///     Number of observations
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        ///     Number of diffrent taxaon
        /// </summary>
        public long TaxonCount { get; set; }

        /// <summary>
        ///     Year
        /// </summary>
        public int Year { get; set; }
    }
}