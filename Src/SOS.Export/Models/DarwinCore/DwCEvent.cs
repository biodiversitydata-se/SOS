using  SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    /// Darwin core event used for csv
    /// </summary>
    public class DwCEvent : DarwinCoreEvent
    {
        /// <summary>
        /// Pointer to core object
        /// </summary>
        public string CoreID { get; set; }
    }
}