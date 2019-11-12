using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Import
{
    /// <summary>
    /// Root config
    /// </summary>
    public class ImportConfiguration
    {
        /// <summary>
        /// Configuration for clam tree service
        /// </summary>
        public ClamTreeServiceConfiguration ClamTreeServiceConfiguration { get; set; }

        /// <summary>
        /// Configuration for Species Portal
        /// </summary>
        public SpeciesPortalConfiguration SpeciesPortalConfiguration { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public MongoDbConfiguration MongoDbConfiguration { get; set; }

        /// <summary>
        /// KUL Service/repository configuration.
        /// </summary>
        public KulServiceConfiguration KulServiceConfiguration { get; set; }

        /// <summary>
        /// taxon attribute service configuration
        /// </summary>
        public TaxonAttributeServiceConfiguration TaxonAttributeServiceConfiguration { get; set; }

        /// <summary>
        /// Taxon service configuration
        /// </summary>
        public TaxonServiceConfiguration TaxonServiceConfiguration { get; set; }
    }
}