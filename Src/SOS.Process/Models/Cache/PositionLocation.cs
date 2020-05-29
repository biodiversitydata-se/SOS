using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Models.Cache
{
    public class PositionLocation
    {
        /// <summary>
        ///     County property
        /// </summary>
        public ProcessedArea County { get; set; }

        /// <summary>
        ///     Municipality property
        /// </summary>
        public ProcessedArea Municipality { get; set; }

        /// <summary>
        ///     Parish property
        /// </summary>
        public ProcessedArea Parish { get; set; }

        /// <summary>
        ///     Province property
        /// </summary>
        public ProcessedArea Province { get; set; }

        /// <summary>
        ///     County property
        /// </summary>
        public bool EconomicZoneOfSweden { get; set; }
    }
}