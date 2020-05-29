using CsvHelper.Configuration;
using SOS.Export.Models.DarwinCore;

namespace SOS.Export.Mappings
{
    /// <summary>
    ///     Mapping of Darwin Core to csv
    /// </summary>
    public class DwCEventMap : ClassMap<DwCEvent>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public DwCEventMap()
        {
            Map(m => m.CoreID).Index(0).Name("coreID");
            Map(m => m.EventID).Index(1).Name("eventID");
            Map(m => m.ParentEventID).Index(2).Name("parentEventID");
            Map(m => m.FieldNumber).Index(3).Name("fieldNumber");
            Map(m => m.EventDate).Index(4).Name("eventDate");
            Map(m => m.EventTime).Index(5).Name("eventTime");
            Map(m => m.StartDayOfYear).Index(6).Name("startDayOfYear");
            Map(m => m.EndDayOfYear).Index(7).Name("endDayOfYear");
            Map(m => m.Year).Index(8).Name("year");
            Map(m => m.Month).Index(9).Name("month");
            Map(m => m.Day).Index(10).Name("day");
            Map(m => m.VerbatimEventDate).Index(11).Name("verbatimEventDate");
            Map(m => m.Habitat).Index(12).Name("habitat");
            Map(m => m.SamplingProtocol).Index(13).Name("samplingProtocol");
            Map(m => m.SampleSizeValue).Index(14).Name("sampleSizeValue");
            Map(m => m.SampleSizeUnit).Index(15).Name("sampleSizeUnit");
            Map(m => m.SamplingEffort).Index(16).Name("samplingEffort");
            Map(m => m.FieldNotes).Index(17).Name("fieldNotes");
            Map(m => m.EventRemarks).Index(18).Name("eventRemarks");
        }
    }
}