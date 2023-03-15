namespace SOS.Lib.Models.Processed.Validation
{
    /// <summary>
    /// Information about event defect
    /// </summary>
    public class EventDefect
    {
        public EventDefect(EventDefectType defectType, string information)
        {
            DefectType = defectType;
            Information = information;
        }

        /// <summary>
        /// Type of defect
        /// </summary>
        public enum EventDefectType
        {
            /// <summary>
            /// Unknown defect
            /// </summary>
            Unknown = 0,            
            /// <summary>
            /// Event is not in Sweden
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
        public EventDefectType DefectType { get; set; }

        /// <summary>
        /// Information about the defect
        /// </summary>
        public string Information { get; set; }
    }
}