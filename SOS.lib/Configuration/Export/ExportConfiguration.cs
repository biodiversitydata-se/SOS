using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Export
{
    /// <summary>
    /// Root config
    /// </summary>
    public class ExportConfiguration
    {
        /// <summary>
        /// Host
        /// </summary>
        public MongoDbConfiguration MongoDbConfiguration { get; set; }
    }
}