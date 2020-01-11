namespace SOS.Process.Models.Cache
{
    public class PositionLocation
    {
        /// <summary>
        /// County property
        /// </summary>
        public Location County { get; set; }

        /// <summary>
        /// Municipality property
        /// </summary>
        public Location Municipality { get; set; }

        /// <summary>
        /// Parish property
        /// </summary>
        public Location Parish { get; set; }

        /// <summary>
        /// Province property
        /// </summary>
        public Location Province { get; set; }

        /// <summary>
        /// County property
        /// </summary>
        public bool EconomicZoneOfSweden { get; set; }
    }
}
