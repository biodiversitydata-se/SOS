namespace SOS.Lib.Models.Verbatim.SpeciesPortal
{
    /// <summary>
    /// Site object
    /// </summary>
    public class Site
    {
        /// <summary>
        /// County of site
        /// </summary>
        public Metadata County { get; set; }

        /// <summary>
        /// Countyry part of site
        /// </summary>
        public Metadata CountryPart { get; set; }

        /// <summary>
        /// Id of site
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of site
        /// </summary>
        public Metadata Municipality { get; set; }

        /// <summary>
        /// Province
        /// </summary>
        public Metadata Province { get; set; }

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
