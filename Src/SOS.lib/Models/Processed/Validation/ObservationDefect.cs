namespace SOS.Lib.Models.Processed.Validation
{
    /// <summary>
    /// Information about observation defect
    /// </summary>
    public class ObservationDefect
    {
        public ObservationDefect(ObservationDefectType defectType, string information)
        {
            DefectType = defectType;
            Information = information;
        }

        /// <summary>
        /// Type of defect
        /// </summary>
        public enum ObservationDefectType
        {
            /// <summary>
            /// Unknown defect
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Taxon was not found
            /// </summary>
            TaxonNotFound,
            /// <summary>
            /// Observation is not in Sweden
            /// </summary>
            LocationOutsideOfSweden,
            /// <summary>
            /// Mandatory information is missing
            /// </summary>
            MissingMandatoryField,
            /// <summary>
            /// Data don't make sense
            /// </summary>
            LogicError,
            /// <summary>
            /// Value out of allowed range
            /// </summary>
            ValueOutOfRange
        }

        /// <summary>
        /// Type of defect
        /// </summary>
        public ObservationDefectType DefectType { get; set; }

        /// <summary>
        /// Information about the defect
        /// </summary>
        public string Information { get; set; }
    }
}