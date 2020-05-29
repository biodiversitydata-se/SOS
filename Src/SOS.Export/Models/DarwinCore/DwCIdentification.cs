using SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    ///     Darwin core Identification used for csv
    /// </summary>
    public class DwCIdentification : DarwinCoreIdentification
    {
        /// <summary>
        ///     Pointer to core object
        /// </summary>
        public string CoreID { get; set; }
    }
}