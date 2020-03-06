using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Configuration.Process
{
    /// <summary>
    /// Root config
    /// </summary>
    public class ProcessConfiguration
    {
        /// <summary>
        /// Host
        /// </summary>
        public MongoDbConfiguration VerbatimDbConfiguration { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public MongoDbConfiguration ProcessedDbConfiguration { get; set; }

        /// <summary>
        /// Field mapping
        /// </summary>
        public FieldMappingConfiguration FieldMapping { get; set; }
    }
}