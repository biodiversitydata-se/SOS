using System.Collections.Generic;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Models.DataValidation
{
    public class DwcaDataValidationReport<TVerbatim, TProcessed>
    {
        public object Settings { get; set; }
        public DwcaDataValidationReportSummary Summary { get; set; }
        public List<ValidObservationTuple<TVerbatim, TProcessed>> ValidObservations { get; set; }
        public List<InvalidObservationTuple<TVerbatim>> InvalidObservations { get; set; }
        public List<DistinctValuesSummary> DictionaryValues { get; set; }
    }

    public class DwcaDataValidationReportSummary
    {
        public int TotalNumberOfObservationsInFile { get; set; }
        public int NrObservationsProcessed { get; set; }
        public int NrValidObservations { get; set; }
        public int NrInvalidObservations { get; set; }
        public List<string> Remarks { get; set; }
        public List<DefectItem> ObservationDefects { get; set; }
    }

    public class DistinctValuesSummary
    {
        public string Term { get; set; }
        public List<DistinctValuesSummaryItem> MappedValues { get; set; }
        public List<DistinctValuesSummaryItem> CustomValues { get; set; }
        public List<ProcessedFieldMapValue> SosVocabulary { get; set; }
    }

    public class DistinctValuesSummaryItem
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public int Count { get; set; }
        public List<string> VerbatimValues { get; set; }
        public string Comment { get; set; }
    }

    public class DefectItem
    {
        public string Defect { get; set; }
        public int Count { get; set; }
    }
}