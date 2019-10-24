using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Import
{
    /// <summary>
    /// Root config
    /// </summary>
    public class ImportConfiguration
    {
        /// <summary>
        /// Connection strings
        /// </summary>
        public ConnectionStrings ConnectionStrings { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public MongoDbConfiguration MongoDbConfiguration { get; set; }
    }
}