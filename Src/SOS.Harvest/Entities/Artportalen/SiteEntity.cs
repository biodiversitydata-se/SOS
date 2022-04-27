namespace SOS.Harvest.Entities.Artportalen
{
    /// <summary>
    ///     Site object
    /// </summary>
    public class SiteEntity
    {
        /// <summary>
        ///     Accuracy in meters
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        ///     Id of diffusion, 0 if no diffusion
        /// </summary>
        public int DiffusionFactor { get; set; }

        /// <summary>
        /// External id
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        ///     Name of county
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id of parent site
        /// </summary>
        public int? ParentSiteId { get; set; }

        /// <summary>
        ///     Parent side name
        /// </summary>
        public string ParentSiteName { get; set; }

        /// <summary>
        ///     Presentation name with Parish
        /// </summary>
        public string PresentationNameParishRegion { get; set; }

        /// <summary>
        /// Id of project
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        ///     Name of site
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     X coordinate of site
        /// </summary>
        public int XCoord { get; set; }

        /// <summary>
        ///     Y coordinate of site
        /// </summary>
        public int YCoord { get; set; }
    }
}