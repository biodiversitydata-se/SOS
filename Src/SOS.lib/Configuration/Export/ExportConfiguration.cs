using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Export
{
    /// <summary>
    ///     Root config
    /// </summary>
    public class ExportConfiguration
    {
        /// <summary>
        ///     Blob storage configuration
        /// </summary>
        public BlobStorageConfiguration BlobStorageConfiguration { get; set; }

        /// <summary>
        ///     Destination file settings
        /// </summary>
        public FileDestination FileDestination { get; set; }

        /// <summary>
        ///     Host
        /// </summary>
        public MongoDbConfiguration ProcessedDbConfiguration { get; set; }

        /// <summary>
        ///     Zend to config
        /// </summary>
        public ZendToConfiguration ZendToConfiguration { get; set; }
    }
}