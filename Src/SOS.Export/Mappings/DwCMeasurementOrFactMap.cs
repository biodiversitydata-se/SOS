using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    ///     Mapping of Darwin Core to csv
    /// </summary>
    public class DwCMeasurementOrFactMap : ClassMap<DwCMeasurementOrFact>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DwCMeasurementOrFactMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreId");
            Map(m => m.MeasurementID).Index(1).Name("measurementID");
            Map(m => m.MeasurementType).Index(2).Name("measurementType");
            Map(m => m.MeasurementValue).Index(3).Name("measurementValue");
            Map(m => m.MeasurementAccuracy).Index(4).Name("measurementAccuracy");
            Map(m => m.MeasurementUnit).Index(5).Name("measurementUnit");
            Map(m => m.MeasurementDeterminedBy).Index(6).Name("measurementDeterminedBy");
            Map(m => m.MeasurementDeterminedDate).Index(7).Name("measurementDeterminedDate");
            Map(m => m.MeasurementMethod).Index(8).Name("measurementMethod");
            Map(m => m.MeasurementRemarks).Index(9).Name("measurementRemarks");
        }
    }
}