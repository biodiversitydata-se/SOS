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
        public ClamServiceConfiguration ClamServiceConfiguration { get; set; }

        /// <summary>
        /// Configuration for Artportalen
        /// </summary>
        public ArtportalenConfiguration ArtportalenConfiguration { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public MongoDbConfiguration VerbatimDbConfiguration { get; set; }

        /// <summary>
        /// KUL Service/repository configuration.
        /// </summary>
        public KulServiceConfiguration KulServiceConfiguration { get; set; }

        /// <summary>
        /// NORS configuration
        /// </summary>
        public MvmServiceConfiguration MvmServiceConfiguration { get; set; }

        /// <summary>
        /// NORS configuration
        /// </summary>
        public NorsServiceConfiguration NorsServiceConfiguration { get; set; }

        /// <summary>
        /// SERS configuration
        /// </summary>
        public SersServiceConfiguration SersServiceConfiguration { get; set; }

        /// <summary>
        /// SHARK configuration
        /// </summary>
        public SharkServiceConfiguration SharkServiceConfiguration { get; set; }

        /// <summary>
        /// taxon attribute service configuration
        /// </summary>
        public TaxonAttributeServiceConfiguration TaxonAttributeServiceConfiguration { get; set; }

        /// <summary>
        /// Taxon service configuration
        /// </summary>
        public TaxonServiceConfiguration TaxonServiceConfiguration { get; set; }

        /// <summary>
        /// Virtual Herbarium service configuration
        /// </summary>
        public VirtualHerbariumServiceConfiguration VirtualHerbariumServiceConfiguration { get; set; }
    }
}