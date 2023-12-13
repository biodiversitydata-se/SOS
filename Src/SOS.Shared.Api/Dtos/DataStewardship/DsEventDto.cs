﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace SOS.Shared.Api.Dtos.DataStewardship
{
    /// <summary>
    /// A specified event when organisms were surveyed according to a specified methodology at a specific place.
    /// </summary>
    [SwaggerSchema("A specified event when organisms were surveyed according to a specified methodology at a specific place.", Required = new[] { "EventID" })]
    public class DsEventDto
    {
        /// <summary>
        /// A unique id for the survey event within a specific dataset hosted by the respective datahosts. A survey event is usually a visit to a survey area/site. 
        /// A survey event can also be a visit to a smaller defined part of a survey area/site or one of several samples from the same place. 
        /// The term survey event can also represent a specific period or season. What eventID is representing is given in the attribute eventType.
        /// </summary>
        [Required]
        [SwaggerSchema("A unique id for the survey event within a specific dataset hosted by the respective datahosts. A survey event is usually a visit to a survey area/site. A survey event can also be a visit to a smaller defined part of a survey area/site or one of several samples from the same place. The term survey event can also represent a specific period or season. What eventID is representing is given in the attribute eventType.")]
        public string EventID { get; set; }

        /// <summary>
        /// States what the survey event (eventID) is representing, e.g. a visit, part visit, sample, period, season etc.
        /// </summary>
        [SwaggerSchema("States what the survey event (eventID) is representing, e.g. a visit, part visit, sample, period, season etc.")]
        public string EventType { get; set; }

        /// <summary>
        /// A unique id that groups several related visits, e.g. all visits to the different subparts within a survey location, or all visits to all survey locations that were made during a season. 
        /// Example: EventID for a survey event at a location becomes the parentID for all the visits to the different subparts. 
        /// EventID for a season becomes the parentID for all the visits to all locations within the survey programme.
        /// </summary>        
        public string ParentEventID { get; set; }

        /// <summary>
        /// Date and Time for when the survey started (local time).
        /// </summary>
        [SwaggerSchema("Date and Time for when the survey started (local time).")]
        public DateTime? EventStartDate { get; set; }

        /// <summary>
        /// Date and Time for when the survey ended (local time).
        /// </summary>
        [SwaggerSchema("Date and Time for when the survey ended (local time).")]
        public DateTime? EventEndDate { get; set; }

        /// <summary>
        /// Start date of the event in the format yyyy-MM-dd.
        /// </summary>
        public string PlainStartDate { get; set; }

        /// <summary>
        /// End date of the event in the format yyyy-MM-dd.
        /// </summary>
        public string PlainEndDate { get; set; }

        /// <summary>
        /// Start time of the event in W. Europe Standard Time formatted as hh:mm.
        /// </summary>
        public string PlainStartTime { get; set; }

        /// <summary>
        /// End time of the event in W. Europe Standard Time formatted as hh:mm. 
        /// </summary>
        public string PlainEndTime { get; set; }

        /// <summary>
        /// States whether the position of a location is protected. The true position for a protected location is shown only to authorized persons. To others the position is shown with diffused coordinates. 
        /// Other information about a protected location can also be limited.
        /// </summary>
        [SwaggerSchema("States whether the position of a location is protected. The true position for a protected location is shown only to authorized persons. To others the position is shown with diffused coordinates. Other information about a protected location can also be limited.")]
        public bool? LocationProtected { get; set; }

        /// <summary>
        /// Information about the survey area/site (geodata).
        /// </summary>
        [SwaggerSchema("Information about the survey area/site (geodata).")]
        public DsLocationDto SurveyLocation { get; set; }

        /// <summary>
        /// Name or code for the person conducting the survey. Several names or codes can be given.
        /// </summary>
        [SwaggerSchema("Name or code for the person conducting the survey. Several names or codes can be given.")]
        public List<string> RecorderCode { get; set; }

        /// <summary>
        /// The organisation that the surveyor represents. Several organisations can be given.
        /// </summary>
        [SwaggerSchema("The organisation that the surveyor represents. Several organisations can be given.")]
        public IEnumerable<DsOrganisationDto> RecorderOrganisation { get; set; }

        /// <summary>
        /// The survey method used at the specific survey event. The monitoring manual or equivalent methodology is referenced in Dataset C.1.2.
        /// </summary>
        [SwaggerSchema("The survey method used at the specific survey event. The monitoring manual or equivalent methodology is referenced in Dataset C.1.2.")]
        public string SamplingProtocol { get; set; }

        /// <summary>
        /// Information about the weather conditions during the survey event.
        /// </summary>
        [SwaggerSchema("Information about the weather conditions during the survey event.")]
        public DsWeatherVariableDto Weather { get; set; }

        /// <summary>
        /// Comment (freetext) about the survey event.
        /// </summary>
        [SwaggerSchema("Comment (freetext) about the survey event.")]
        public string EventRemarks { get; set; }

        /// <summary>
        /// States whether any of the sought after organisms were observed during the survey event or not. True means that none of the sought after organisms were observed at all.
        /// </summary>
        [SwaggerSchema("States whether any of the sought after organisms were observed during the survey event or not. True means that none of the sought after organisms were observed at all.")]
        public bool? NoObservations { get; set; }

        /// <summary>
        /// Attached information about the survey event, e.g. media files like images, sound recordings, maps etc.
        /// </summary>
        [SwaggerSchema("Attached information about the survey event, e.g. media files like images, sound recordings, maps etc.")]
        public IEnumerable<DsAssociatedMediaDto> AssociatedMedia { get; set; }

        /// <summary>
        /// Information regarding the data set
        /// </summary>
        [SwaggerSchema("Dataset")]
        public DsDatasetInfoDto Dataset { get; set; }

        /// <summary>
        /// A list of unique identities of the occurances made during an event
        /// </summary>
        [SwaggerSchema("A list of unique identities of the occurances made during an event")]
        public IEnumerable<string> OccurrenceIds { get; set; }
    }
}
