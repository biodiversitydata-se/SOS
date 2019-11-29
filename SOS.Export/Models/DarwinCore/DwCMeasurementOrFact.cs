using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Export.Models.DarwinCore
{
    /// <summary>
    /// Darwin core Measurement Or Fact used for csv
    /// </summary>
    public class DwCMeasurementOrFact : DarwinCoreMeasurementOrFact
    {
        /// <summary>
        /// Pointer to core object
        /// </summary>
        public string CoreID { get; set; }
    }
}