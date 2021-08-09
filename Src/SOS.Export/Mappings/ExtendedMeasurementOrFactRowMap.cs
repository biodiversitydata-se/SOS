using CsvHelper.Configuration;
using SOS.Lib.IO.Csv.Converters;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    ///     Mapping of Darwin Core to csv.
    ///     http://rs.gbif.org/extension/obis/extended_measurement_or_fact.xml
    /// </summary>
    public class ExtendedMeasurementOrFactRowMap : ClassMap<ExtendedMeasurementOrFactRow>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ExtendedMeasurementOrFactRowMap()
        {
            Map(m => m.OccurrenceID).Index(0).Name("occurrenceID");
            Map(m => m.MeasurementID).Index(1).Name("measurementID");
            Map(m => m.MeasurementType).Index(2).Name("measurementType")
                .TypeConverter<LineBreakTabStringConverter<string>>();
            Map(m => m.MeasurementTypeID).Index(3).Name("measurementTypeID");
            Map(m => m.MeasurementValue).Index(4).Name("measurementValue")
                .TypeConverter<LineBreakTabStringConverter<string>>();
            Map(m => m.MeasurementValueID).Index(5).Name("measurementValueID");
            Map(m => m.MeasurementAccuracy).Index(6).Name("measurementAccuracy");
            Map(m => m.MeasurementUnit).Index(7).Name("measurementUnit")
                .TypeConverter<LineBreakTabStringConverter<string>>();
            Map(m => m.MeasurementUnitID).Index(8).Name("measurementUnitID");
            Map(m => m.MeasurementDeterminedDate).Index(9).Name("measurementDeterminedDate");
            Map(m => m.MeasurementDeterminedBy).Index(10).Name("measurementDeterminedBy");
            Map(m => m.MeasurementRemarks).Index(11).Name("measurementRemarks")
                .TypeConverter<LineBreakTabStringConverter<string>>();
            Map(m => m.MeasurementMethod).Index(12).Name("measurementMethod")
                .TypeConverter<LineBreakTabStringConverter<string>>();
        }
    }
}