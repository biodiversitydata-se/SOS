using SOS.DataStewardship.Api.Models;
using SOS.DataStewardship.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{
    /// <summary>
    /// A specified event when organisms were surveyed according to a specified methodology at a specific place.
    /// </summary>
    [DataContract]
    public class EventModel
    { 
        /// <summary>
        /// A unique id for the survey event within a specific dataset hosted by the respective datahosts. A survey event is usually a visit to a survey area/site. A survey event can also be a visit to a smaller defined part of a survey area/site or one of several samples from the same place. The term survey event can also represent a specific period or season. What eventID is representing is given in the attribute eventType.
        /// </summary>
        [Required]
        [DataMember(Name="eventID")]
        public string EventID { get; set; }

        /// <summary>
        /// States what the survey event (eventID) is representing, e.g. a visit, part visit, sample, period, season etc.
        /// </summary>
        [DataMember(Name="eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// A unique id that groups several related visits, e.g. all visits to the different subparts within a survey location, or all visits to all survey locations that were made during a season. Example: EventID for a survey event at a location becomes the parentID for all the visits to the different subparts. EventID for a season becomes the parentID for all the visits to all locations within the survey programme.
        /// </summary>
        [DataMember(Name="parentEventID")]
        public string ParentEventID { get; set; }

        /// <summary>
        /// Date and Time for when the survey started (local time).
        /// </summary>
        /// <value>Date and Time for when the survey started (local time).</value>
        [Required]
        [DataMember(Name="eventStartDate")]
        public DateTime? EventStartDate { get; set; }

        /// <summary>
        /// Date and Time for when the survey ended (local time).
        /// </summary>
        [Required]
        [DataMember(Name="eventEndDate")]
        public DateTime? EventEndDate { get; set; }

        /// <summary>
        /// States whether the position of a location is protected. The true position for a protected location is shown only to authorized persons. To others the position is shown with diffused coordinates. Other information about a protected location can also be limited.
        /// </summary>
        [Required]
        [DataMember(Name="locationProtected")]
        public bool? LocationProtected { get; set; }

        /// <summary>
        /// Information about the survey area/site (geodata).
        /// </summary>
        [Required]
        [DataMember(Name="surveyLocation")]
        public Location SurveyLocation { get; set; }        

        /// <summary>
        /// Name or code for the person conducting the survey. Several names or codes can be given.
        /// </summary>
        [DataMember(Name="recorderCode")]
        public List<string> RecorderCode { get; set; }

        /// <summary>
        /// The organisation that the surveyor represents. Several organisations can be given.
        /// </summary>
        [DataMember(Name="recorderOrganisation")]
        public List<Organisation> RecorderOrganisation { get; set; }

        /// <summary>
        /// The survey method used at the specific survey event. The monitoring manual or equivalent methodology is referenced in Dataset C.1.2.
        /// </summary>
        [DataMember(Name="samplingProtocol")]
        public string SamplingProtocol { get; set; }

        /// <summary>
        /// Information about the weather conditions during the survey event.
        /// </summary>
        [DataMember(Name="weather")]
        public WeatherVariable Weather { get; set; }

        /// <summary>
        /// Comment (freetext) about the survey event.
        /// </summary>
        [DataMember(Name="eventRemarks")]
        public string EventRemarks { get; set; }

        /// <summary>
        /// States whether any of the sought after organisms were observed during the survey event or not. \&quot;Sant\&quot; (i.e. True) means that none of the sought after organisms were observed at all.
        /// </summary>
        [DataMember(Name="noObservations")]
        public bool? NoObservations { get; set; }

        /// <summary>
        /// Attached information about the survey event, e.g. media files like images, sound recordings, maps etc.
        /// </summary>
        [DataMember(Name="associatedMedia")]
        public List<AssociatedMedia> AssociatedMedia { get; set; }

        /// <summary>
        /// Dataset
        /// </summary>
        [DataMember(Name="Dataset")]
        public EventDataset Dataset { get; set; }

        /// <summary>
        /// A list of unique identities of the occurances made during an event
        /// </summary>
        [DataMember(Name="OccurrenceIds")]
        public List<string> OccurrenceIds { get; set; }
    }
}