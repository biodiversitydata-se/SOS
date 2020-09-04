using System.Collections.Generic;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Models.DataValidation
{
    public class DwcaDataValidationReport<TVerbatim, TProcessed>
    {
        public object Settings { get; set; }
        public DwcaDatavalidationReportDescription Descriptions => new DwcaDatavalidationReportDescription();
        public DwcaDataValidationReportSummary Summary { get; set; }
        public List<ValidObservationTuple<TVerbatim, TProcessed>> ValidObservations { get; set; }
        public List<InvalidObservationTuple<TVerbatim>> InvalidObservations { get; set; }
        public List<DistinctValuesSummary> DictionaryValues { get; set; }
    }

    public class DwcaDatavalidationReportDescription
    {
        public string TotalNumberOfObservationsInFile => "The total number of observations in the DwC-A file.";
        public string NrObservationsProcessed => "The number of observations that have been processed to create this report.";
        public string NrValidObservations => "The number of observations that could be processed without validation errors.";
        public string NrInvalidObservations => "The number of observations that could not be processed due to validation errors. For example, that no matching taxon in Dyntaxa was found, or that the observation is outside Sweden's borders.";
        public string Remarks => "Remarks on data that is missing in the file, which could be included to improve the quality of the processing.";
        public string ObservationDefects => "Validation error information.";
        public string ValidObservations => "A listing of valid observations (default is 100). For each observation, the following is shown: VerbatimObservation = raw data from the DwC-A CSV files. ProcessedObservation = this is what the observation looks like in the SOS database after it has been processed. DwcExport = this is what the observation looks like when the processed observation is exported to DwC.";
        public string InvalidObservations => "A listing of invalid observations (default is 100). For each observation, the following is shown: VerbatimObservation = raw data from the DwC-A CSV files. ProcessedObservationDefects = a list of defects.";
        public string DictionaryValues => "Some fields have a predefined dictionary in SOS, for example the fields BasisOfRecord, Gender, LifeStage, ValidationStatus, OccurrenceStatus & Municipality. This is a summary of what values that could be mapped to those dictionary entries.";
    }

    public class DwcaDataValidationReportSummary
    {
        public string ReportCreatedDate { get; set; }
        public int TotalNumberOfObservationsInFile { get; set; }
        public int NrObservationsProcessed { get; set; }
        public int NrValidObservations { get; set; }
        public int NrInvalidObservations { get; set; }
        public List<string> Remarks { get; set; }
        public List<DefectItem> ObservationDefects { get; set; }
        public List<string> NonMatchingScientificNames { get; set; }
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