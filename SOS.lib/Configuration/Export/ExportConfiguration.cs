using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Export
{
    /// <summary>
    /// Root config
    /// </summary>
    public class ExportConfiguration
    {
        /// <summary>
        /// Destination file settings
        /// </summary>
        public FileDestination FileDestination { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public MongoDbConfiguration MongoDbConfiguration { get; set; }
    }
}