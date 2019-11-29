namespace SOS.Lib.Models.Processed.DarwinCore
{
    /// <summary>
    /// A measurement of or fact about an rdfs:Resource (http://www.w3.org/2000/01/rdf-schema#Resource).
    /// Resources can be thought of as identifiable records or instances of classes and may include, but need not be limited to
    /// dwc:Occurrence, dwc:Organism, dwc:MaterialSample, dwc:Event, dwc:Location, dwc:GeologicalContext, dwc:Identification, or dwc:Taxon.
    /// </summary>
    /// <example>
    /// The weight of an organism in grams. The number of placental scars. Surface water temperature in Celsius.
    /// </example>
    public class DarwinCoreMeasurementOrFact
    {
        /// <summary>
        /// The description of the potential error associated with the measurementValue.
        /// </summary>
        /// <example>
        /// 0.01, normal distribution with variation of 2 m
        /// </example>
        public string MeasurementAccuracy { get; set; }

        /// <summary>
        /// A list (concatenated and separated) of names of people, groups, or organizations who determined the value of the MeasurementOrFact.
        /// Recommended best practice is to separate the values in a list with space vertical bar space ( | ).
        /// </summary>
        /// <example>
        /// Rob Guralnick, Peter Desmet | Stijn Van Hoey
        /// </example>
        public string MeasurementDeterminedBy { get; set; }

        /// <summary>
        /// The date on which the MeasurementOrFact was made.
        /// Recommended best practice is to use a date that conforms to ISO 8601:2004(E).
        /// </summary>
        /// <example>
        /// 1963-03-08T14:07-0600 (8 Mar 1963 at 2:07pm in the time zone six hours earlier than UTC). 2009-02-20T08:40Z (20 February 2009 8:40am UTC). 2018-08-29T15:19 (3:19pm local time on 29 August 2018). 1809-02-12 (some time during 12 February 1809). 1906-06 (some time in June 1906). 1971 (some time in the year 1971). 2007-03-01T13:00:00Z/2008-05-11T15:30:00Z (some time during the interval between 1 March 2007 1pm UTC and 11 May 2008 3:30pm UTC). 1900/1909 (some time during the interval between the beginning of the year 1900 and the end of the year 1909). 2007-11-13/15 (some time in the interval between 13 November 2007 and 15 November 2007).
        /// </example>
        public string MeasurementDeterminedDate { get; set; }

        /// <summary>
        /// An identifier for the MeasurementOrFact (information pertaining to measurements, facts, characteristics, or assertions).
        /// May be a global unique identifier or an identifier specific to the data set.
        /// </summary>
        /// <example>
        /// 9c752d22-b09a-11e8-96f8-529269fb1459
        /// </example>
        public string MeasurementID { get; set; }

        /// <summary>
        /// A description of or reference to (publication, URI) the method or protocol used to determine the measurement, fact, characteristic, or assertion.
        /// </summary>
        /// <example>
        /// minimum convex polygon around burrow entrances (for a home range area). barometric altimeter (for an elevation).
        /// </example>
        public string MeasurementMethod { get; set; }

        /// <summary>
        /// Comments or notes accompanying the MeasurementOrFact.
        /// </summary>
        /// <example>
        /// tip of tail missing
        /// </example>
        public string MeasurementRemarks { get; set; }

        /// <summary>
        /// The nature of the measurement, fact, characteristic, or assertion.
        /// Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        /// <example>
        /// tail length, temperature, trap line length, survey area, trap type
        /// </example>
        public string MeasurementType { get; set; }

        /// <summary>
        /// The units associated with the measurementValue
        /// Recommended best practice is to use the International System of Units (SI).
        /// </summary>
        /// <example>
        /// mm, C, km, ha
        /// </example>
        public string MeasurementUnit { get; set; }

        /// <summary>
        /// The value of the measurement, fact, characteristic, or assertion.
        /// </summary>
        /// <example>
        /// 45, 20, 1, 14.5, UV-light
        /// </example>
        public string MeasurementValue { get; set; }
    }
}
