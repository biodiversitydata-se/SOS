using SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    ///     Darwin core Occurrence used for csv
    /// </summary>
    public class DwCOccurrence : DarwinCoreOccurrence
    {
        /// <summary>
        ///     Pointer to core object
        /// </summary>
        public string CoreID { get; set; }
    }
}