using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Import
{
    /// <summary>
    ///     Root config
    /// </summary>
    public class ImportConfiguration
    {
        /// <summary>
        ///     Configuration for Artportalen
        /// </summary>
        public ArtportalenConfiguration ArtportalenConfiguration { get; set; }
        
        /// <summary>
        /// Biologg configuration
        /// </summary>
        public BiologConfiguration BiologConfiguration { get; set; }

        /// <summary>
        ///     Configuration for DwC-A
        /// </summary>
        public DwcaConfiguration DwcaConfiguration { get; set; }
        
        /// <summary>
        ///     Configuration for clam tree service
        /// </summary>
        public ClamServiceConfiguration ClamServiceConfiguration { get; set; }

        /// <summary>
        ///     Configuration for area import.
        /// </summary>
        public AreaHarvestConfiguration AreaHarvestConfiguration { get; set; }

        /// <summary>
        ///     Configuration for GeoRegion API.
        /// </summary>
        public GeoRegionApiConfiguration GeoRegionApiConfiguration { get; set; }

        /// <summary>
        /// Fish data configuration
        /// </summary>
        public FishDataServiceConfiguration FishDataServiceConfiguration { get; set; }

        /// <summary>
        ///     KUL Service/repository configuration.
        /// </summary>
        public KulServiceConfiguration KulServiceConfiguration { get; set; }

        /// <summary>
        ///     NORS configuration
        /// </summary>
        public MvmServiceConfiguration MvmServiceConfiguration { get; set; }

        /// <summary>
        ///     NORS configuration
        /// </summary>
        public NorsServiceConfiguration NorsServiceConfiguration { get; set; }

        /// <summary>
        /// Observations database configuration
        /// </summary>
        public ObservationDatabaseConfiguration ObservationDatabaseConfiguration { get; set; }
        /// <summary>
        ///     SERS configuration
        /// </summary>
        public SersServiceConfiguration SersServiceConfiguration { get; set; }

        /// <summary>
        ///     SHARK configuration
        /// </summary>
        public SharkServiceConfiguration SharkServiceConfiguration { get; set; }

        /// <summary>
        ///     Virtual Herbarium service configuration
        /// </summary>
        public VirtualHerbariumServiceConfiguration VirtualHerbariumServiceConfiguration { get; set; }

        /// <summary>
        ///     iNaturalist Service/repository configuration.
        /// </summary>
        public iNaturalistServiceConfiguration iNaturalistServiceConfiguration { get; set; }

        /// <summary>
        /// Taxon list service configuration.
        /// </summary>
        public TaxonListServiceConfiguration TaxonListServiceConfiguration { get; set; }
    }
}