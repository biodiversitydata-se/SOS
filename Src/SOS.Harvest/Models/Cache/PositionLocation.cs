using SOS.Lib.Models.Processed.Observation;

namespace SOS.Harvest.Models.Cache
{
    public class PositionLocation
    {
        /// <summary>
        ///     County property
        /// </summary>
        public Area County { get; set; }

        /// <summary>
        ///     Municipality property
        /// </summary>
        public Area Municipality { get; set; }

        /// <summary>
        ///     Parish property
        /// </summary>
        public Area Parish { get; set; }

        /// <summary>
        ///     Province property
        /// </summary>
        public Area Province { get; set; }

        /// <summary>
        ///     County property
        /// </summary>
        public bool EconomicZoneOfSweden { get; set; }
    }
}