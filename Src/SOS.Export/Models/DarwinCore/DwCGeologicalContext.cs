using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    /// Darwin core Geological Context used for csv
    /// </summary>
    public class DwCGeologicalContext : DarwinCoreGeologicalContext
    {
        /// <summary>
        /// Pointer to core object
        /// </summary>
        public string CoreID { get; set; }
    }
}