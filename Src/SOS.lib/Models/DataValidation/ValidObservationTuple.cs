using System.Collections.Generic;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Lib.Models.DataValidation
{
    public class ValidObservationTuple<TVerbatim, TProcessed>
    {
        public TVerbatim VerbatimObservation { get; set; }
        public TProcessed ProcessedObservation { get; set; }
        public DwcExport DwcExport { get; set; }
    }

    public class DwcExport
    {
        public DarwinCore.DarwinCore Observation { get; set; }
        public DwcExportExtensions Extensions { get; set; }
    }

    public class DwcExportExtensions
    {
        public IEnumerable<ExtendedMeasurementOrFactRow> ExtendedMeasurementOrFacts { get; set; }
    }
}