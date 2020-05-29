namespace SOS.Lib.Models.Verbatim.DarwinCore
{
    /// <summary>
    ///     Darwin Core Measurement Or Facts
    /// </summary>
    public class DwcMeasurementOrFact
    {
        /// <summary>
        ///     An identifier for the MeasurementOrFact (information pertaining to
        ///     measurements, facts, characteristics, or assertions).
        ///     May be a global unique identifier or an identifier specific to the data set.
        /// </summary>
        public string MeasurementID { get; set; }

        /// <summary>
        ///     The nature of the measurement, fact, characteristic, or assertion.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string MeasurementType { get; set; }

        /// <summary>
        ///     The value of the measurement, fact, characteristic, or assertion.
        /// </summary>
        public string MeasurementValue { get; set; }

        /// <summary>
        ///     The description of the potential error associated with the measurementValue.
        /// </summary>
        public string MeasurementAccuracy { get; set; }

        /// <summary>
        ///     The units associated with the measurementValue.
        ///     Recommended best practice is to use the International System of Units (SI).
        /// </summary>
        public string MeasurementUnit { get; set; }

        /// <summary>
        ///     The date on which the MeasurementOrFact was made.
        ///     Recommended best practice is to use an encoding scheme, such as ISO 8601:2004(E).
        /// </summary>
        public string MeasurementDeterminedDate { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of names of people, groups, or
        ///     organizations who determined the value of the MeasurementOrFact.
        /// </summary>
        public string MeasurementDeterminedBy { get; set; }

        /// <summary>
        ///     A description of or reference to (publication, URI) the method or protocol
        ///     used to determine the measurement, fact, characteristic, or assertion.
        /// </summary>
        public string MeasurementMethod { get; set; }

        /// <summary>
        ///     Comments or notes accompanying the MeasurementOrFact.
        /// </summary>
        public string MeasurementRemarks { get; set; }

        public override string ToString()
        {
            return $"Type: \"{MeasurementType}\", Value: \"{MeasurementValue}\", Unit: \"{MeasurementUnit}\"";
        }
    }
}