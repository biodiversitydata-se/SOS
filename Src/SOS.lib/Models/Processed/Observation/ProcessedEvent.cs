using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// This class contains event information about a species observation
    /// </summary>
    public class ProcessedEvent 
    {
        /// <summary>
        /// Biotope.
        /// </summary>
        /// <remarks>
        /// This value is field mapped.
        /// </remarks>
        public ProcessedFieldMapValue BiotopeId { get; set; }

        /// <summary>
        /// Description of biotope
        /// </summary>
        public string BiotopeDescription { get; set; }

        /// <summary>
        /// End date/time of the event
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// A list (concatenated and separated) of identifiers
        /// (publication, global unique identifier, URI) of
        /// media associated with the Occurrence.
        /// This property is currently not used.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string Id { get; set; }

        /// <summary>
        /// Comments or notes about the Event.
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// One of a) an indicator of the existence of, b) a
        /// reference to (publication, URI), or c) the text of
        /// notes taken in the field about the Event.
        /// This property is currently not used.
        /// </summary>
        public string FieldNotes { get; set; }

        /// <summary>
        /// An identifier given to the event in the field. Often 
        /// serves as a link between field notes and the Event.
        /// This property is currently not used.
        /// </summary>
        public string FieldNumber { get; set; }

        /// <summary>
        /// An identifier for the set of information associated with an Event (something that occurs at a place and time).
        /// May be a global unique identifier or an identifier specific to the data set.
        /// </summary>
        /// <example>
        /// A1 (parentEventID to identify the main Whittaker Plot in nested samples, each with its own eventID - A1:1, A1:2).
        /// </example>
        public string ParentId { get; set; }

        /// <summary>
        /// Quality of substrate
        /// </summary>
        public int? QuantityOfSubstrate { get; set; }

        /// <summary>
        /// The amount of effort expended during an Event.
        /// This property is currently not used.
        /// </summary>
        public string SamplingEffort { get; set; }

        /// <summary>
        /// The name of, reference to, or description of the
        /// method or protocol used during an Event.
        /// This property is currently not used.
        /// </summary>
        public string SamplingProtocol { get; set; }

        /// <summary>
        /// The unit of measurement of the size (time duration, length, area, or volume) of a sample in a sampling event.
        /// A sampleSizeUnit must have a corresponding sampleSizeValue, e.g., 5 for sampleSizeValue with metre for sampleSizeUnit.
        /// </summary>
        /// <example>
        /// minute, hour, day, metre, square metre, cubic metre
        /// </example>
        public string SampleSizeUnit { get; set; }

        /// <summary>
        /// A numeric value for a measurement of the size (time duration, length, area, or volume) of a sample in a sampling event.
        /// </summary>
        /// <example>
        /// 5 for sampleSizeValue with metre for sampleSizeUnit.
        /// </example>
        public string SampleSizeValue { get; set; }

        /// <summary>
        /// Start date/time of the event
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Substrate.
        /// </summary>
        /// <remarks>
        /// This value is field mapped.
        /// </remarks>
        public ProcessedFieldMapValue SubstrateId { get; set; }

        /// <summary>
        /// Description of substrate
        /// </summary>
        public string SubstrateDescription { get; set; }

        /// <summary>
        /// Description of substrate species
        /// </summary>
        public string SubstrateSpeciesDescription { get; set; }

        /// <summary>
        /// Substrate taxon id
        /// </summary>
        public int? SubstrateSpeciesId { get; set; }

        /// <summary>
        /// Verbatim end date
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? VerbatimEndDate { get; set; }

        /// <summary>
        /// Verbatim start date
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? VerbatimStartDate { get; set; }
    }
}
