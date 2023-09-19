using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Models.Cache
{
    public class PositionLocation
    {
        /// <summary>
        /// Atlas square 5x5 km
        /// </summary>
        public Area Atlas5x5 { get; set; }

        /// <summary>
        ///     Country region property
        /// </summary>
        public Area CountryRegion { get; set; }

        /// <summary>
        /// County property
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