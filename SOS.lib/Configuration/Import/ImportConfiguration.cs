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
    }
}