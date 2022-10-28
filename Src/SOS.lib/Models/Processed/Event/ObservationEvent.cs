using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Interfaces;
using static SOS.Lib.Models.Processed.Dataset.ObservationDataset;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Models.Processed.Event
{
    public class ObservationEvent : IEntity<string>
    {
        /// <summary>
        ///     Unique id.
        /// </summary>
        /// <remarks>
        /// Omit to automatically generate an id on insert (best performance).
        /// </remarks>
        public string Id { get; set; }

        /// <summary>
        /// A unique id for the survey event within a specific dataset hosted by the respective datahosts. A survey event is usually a visit to a survey area/site. A survey event can also be a visit to a smaller defined part of a survey area/site or one of several samples from the same place. The term survey event can also represent a specific period or season. What eventID is representing is given in the attribute eventType.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// States what the survey event (eventID) is representing, e.g. a visit, part visit, sample, period, season etc.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Dataset
        /// </summary>
        public EventDataset Dataset { get; set; }

        /// <summary>
        /// A unique id that groups several related visits, e.g. all visits to the different subparts within a survey location, or all visits to all survey locations that were made during a season. Example: EventID for a survey event at a location becomes the parentID for all the visits to the different subparts. EventID for a season becomes the parentID for all the visits to all locations within the survey programme.
        /// </summary>        
        public string ParentEventId { get; set; }

        /// <summary>
        /// Date and Time for when the survey started (local time).
        /// </summary>
        /// <value>Date and Time for when the survey started (local time).</value>        
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Date and Time for when the survey ended (local time).
        /// </summary>        
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// States whether the position of a location is protected. The true position for a protected location is shown only to authorized persons. To others the position is shown with diffused coordinates. Other information about a protected location can also be limited.
        /// </summary>
        /// <remarks>
        /// todo - move this to Location?
        /// </remarks>
        public bool? LocationProtected { get; set; }

        /// <summary>
        /// Information about the survey area/site (geodata).
        /// </summary>        
        public Location Location { get; set; }

        /// <summary>
        /// Name or code for the person conducting the survey. Several names or codes can be given.
        /// </summary>
        public List<string> RecorderCode { get; set; }

        /// <summary>
        /// The organisation that the surveyor represents. Several organisations can be given.
        /// </summary>
        public List<Organisation> RecorderOrganisation { get; set; }

        /// <summary>
        ///    DiscoveryMethod from Artportalen.
        /// </summary>
        public VocabularyValue DiscoveryMethod { get; set; }

        /// <summary>
        ///     A category or description of the habitat in which the Event occurred.
        /// </summary>
        public string Habitat { get; set; }

        /// <summary>
        /// The survey method used at the specific survey event. The monitoring manual or equivalent methodology is referenced in Dataset C.1.2.
        /// </summary>
        public string SamplingProtocol { get; set; }

        /// <summary>
        ///     The amount of effort expended during an Event.
        /// </summary>
        public string SamplingEffort { get; set; }

        /// <summary>
        ///     The unit of measurement of the size (time duration, length, area, or volume) of a sample in a sampling event.
        ///     A sampleSizeUnit must have a corresponding sampleSizeValue, e.g., 5 for sampleSizeValue with metre for
        ///     sampleSizeUnit.
        /// </summary>
        /// <example>
        ///     minute, hour, day, metre, square metre, cubic metre.
        /// </example>
        public string SampleSizeUnit { get; set; }

        /// <summary>
        ///     A numeric value for a measurement of the size (time duration, length, area, or volume) of a sample in a sampling
        ///     event.
        /// </summary>
        /// <example>
        ///     5 for sampleSizeValue with metre for sampleSizeUnit.
        /// </example>
        public string SampleSizeValue { get; set; }

        /// <summary>
        /// Information about the weather conditions during the survey event.
        /// </summary>
        public WeatherVariable Weather { get; set; }

        /// <summary>
        /// Comment (freetext) about the survey event.
        /// </summary>
        public string EventRemarks { get; set; }
        
        /// <summary>
        /// States whether any of the sought after organisms were observed during the survey event or not. \&quot;Sant\&quot; (i.e. True) means that none of the sought after organisms were observed at all.
        /// </summary>
        public bool NoObservations { get; set; }

        /// <summary>
        /// Attached information about the survey event, e.g. media files like images, sound recordings, maps etc.
        /// </summary>
        public List<AssociatedMedia> AssociatedMedia { get; set; }

        /// <summary>
        ///     Multimedia associated with the event.
        /// </summary>
        public ICollection<Multimedia> Media { get; set; }

        /// <summary>
        ///     Measurement or facts associated with the event.
        /// </summary>
        public ICollection<ExtendedMeasurementOrFact> MeasurementOrFacts { get; set; }

        /// <summary>
        /// A list of unique identities of the occurances made during an event
        /// </summary>
        public List<string> OccurrenceIds { get; set; }

        public class EventDataset
        {
            /// <summary>
            /// Identifier
            /// </summary>
            public string Identifier { get; set; }

            /// <summary>
            /// Title
            /// </summary>
            public string Title { get; set; }
        }
    }
}