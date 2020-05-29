using SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    ///     Darwin core Material Sample used for csv
    /// </summary>
    public class DwCMaterialSample : DarwinCoreMaterialSample
    {
        /// <summary>
        ///     Pointer to core object
        /// </summary>
        public string CoreID { get; set; }
    }
}