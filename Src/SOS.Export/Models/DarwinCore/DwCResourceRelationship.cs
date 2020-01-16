using  SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    /// Darwin core Relationship used for csv
    /// </summary>
    public class DwCResourceRelationship : DarwinCoreResourceRelationship
    {
        /// <summary>
        /// Pointer to core object
        /// </summary>
        public string CoreID { get; set; }
    }
}