namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon Risk Assessment
    /// </summary>
    public class TaxonInvasiveInfo
    {
        /// <summary>
        /// True if alien in sweden according to EU Regulation 1143/2014
        /// </summary>
        public bool IsInvasiveAccordingToEuRegulation { get; set; }

        /// <summary>
        /// True if invasive in sweden 
        /// </summary>
        public bool IsInvasiveInSweden { get; set; }

        /// <summary>
        /// Risk assessment category
        /// </summary>
        public string RiskAssessmentCategory { get; set; }
    }
}