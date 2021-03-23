
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Process;

namespace SOS.Lib.Configuration.Export
{
    /// <summary>
    ///     Root config
    /// </summary>
    public class ExportConfiguration
    {
        /// <summary>
        /// DOI meta data
        /// </summary>
        public DOIConfiguration DOIConfiguration { get; set; }
     
        /// <summary>
        ///     Destination file settings
        /// </summary>
        public FileDestination FileDestination { get; set; }

        /// <summary>
        /// DwC-A file creation configuration.
        /// </summary>
        public DwcaFilesCreationConfiguration DwcaFilesCreationConfiguration { get; set; }

        /// <summary>
        ///     Zend to config
        /// </summary>
        public ZendToConfiguration ZendToConfiguration { get; set; }

        /// <summary>
        /// Vocabulary Configuration
        /// </summary>
        public VocabularyConfiguration VocabularyConfiguration { get; set; }

        /// <summary>
        /// User service config
        /// </summary>
        public UserServiceConfiguration UserServiceConfiguration { get; set; }
    }
}