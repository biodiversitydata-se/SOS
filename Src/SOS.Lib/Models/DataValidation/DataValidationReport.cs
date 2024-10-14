using System.Collections.Generic;

namespace SOS.Lib.Models.DataValidation
{
    public class DataValidationReport<TVerbatim, TProcessed>
    {
        public object Settings { get; set; }
        public DatavalidationReportDescription Descriptions => new DatavalidationReportDescription();
        public DataValidationReportSummary Summary { get; set; }
        public List<ValidObservationTuple<TVerbatim, TProcessed>> ValidObservations { get; set; }
        public List<InvalidObservationTuple<TVerbatim>> InvalidObservations { get; set; }
        public List<DistinctValuesSummary> DictionaryValues { get; set; }
    }

    public class DatavalidationReportDescription
    {
        public string TotalNumberOfObservationsInDb => "The total number of observations stored in SOS database.";
        public string NrObservationsProcessed => "The number of observations that have been processed to create this report.";
        public string NrValidObservations => "The number of observations that could be processed without validation errors.";
        public string NrInvalidObservations => "The number of observations that could not be processed due to validation errors. For example, that no matching taxon in Dyntaxa was found, or that the observation is outside Sweden's borders.";
        public string Remarks => "Remarks on data that is missing, which could be included to improve the quality of the processing.";
        public string ObservationDefects => "Validation error information.";
        public string ValidObservations => "A listing of valid observations (default is 100). For each observation, the following is shown: VerbatimObservation = raw data. ProcessedObservation = this is what the observation looks like in the SOS database after it has been processed. DwcExport = this is what the observation looks like when the processed observation is exported to Darwin Core (DwC).";
        public string InvalidObservations => "A listing of invalid observations (default is 100). For each observation, the following is shown: VerbatimObservation = raw data. ProcessedObservationDefects = a list of defects.";
        public string DictionaryValues => "Some fields have a predefined dictionary in SOS, for example the fields BasisOfRecord, Sex, LifeStage, ValidationStatus, OccurrenceStatus & Municipality. This is a summary of what values that could be mapped to those dictionary entries.";
    }

    public class DataValidationReportSummary
    {
        public string ReportCreatedDate { get; set; }
        public long TotalNumberOfObservationsInDb { get; set; }
        public int NrObservationsProcessed { get; set; }
        public int NrValidObservations { get; set; }
        public int NrInvalidObservations { get; set; }
        public List<string> Remarks { get; set; }
        public List<DefectItem> ObservationDefects { get; set; }
        public List<string> NonMatchingScientificNames { get; set; }
        public List<string> NonMatchingTaxonIds { get; set; }
    }
}