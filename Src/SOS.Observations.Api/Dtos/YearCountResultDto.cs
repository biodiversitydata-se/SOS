namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Result returned year aggregation
    /// </summary>
    public class YearCountResultDto
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
