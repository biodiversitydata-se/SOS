using  SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    /// Darwin core location used for csv
    /// </summary>
    public class DwCTaxon : DarwinCoreTaxon
    {
        /// <summary>
        /// Pointer to core object
        /// </summary>
        public string CoreID { get; set; }
    }
}