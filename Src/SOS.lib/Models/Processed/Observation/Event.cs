using System;
using System.Collections.Generic;
using System.Globalization;
using SOS.Lib.Helpers;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Event information about a species observation.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Divide year in 48 weeks
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private int? CalculateHistogramWeek(DateTime? date)
        {
            if (!date.HasValue)
            {
                return null!;
            }
            date = date.Value.ToLocalTime();
            var cal = new GregorianCalendar();
            return ((date.Value.Month - 1) * 4) + (int)Math.Ceiling(date.Value.Day / Math.Round((double)cal.GetDaysInMonth(date.Value.Year, date.Value.Month) / 4));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Event()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public Event(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && startDate.Value.Kind == DateTimeKind.Unspecified)
            {
                startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Local);
            }
            if (endDate.HasValue && endDate.Value.Kind == DateTimeKind.Unspecified)
            {
                endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Local);
            }

            EndDate = endDate?.ToUniversalTime();
            StartDate = startDate?.ToUniversalTime();            
            VerbatimEventDate = DwcFormatter.CreateDateIntervalString(startDate?.ToLocalTime(), endDate?.ToLocalTime());

            if (startDate != null)
            {
                var startDateLocal = startDate.Value.ToLocalTime();
                PlainStartDate = startDateLocal.ToString("yyyy-MM-dd");
                PlainStartTime = startDateLocal.ToString("HH\\:mm");
                StartYear = startDateLocal.Year;
                StartMonth = startDateLocal.Month;
                StartDay = startDateLocal.Day;
            }

            if (endDate != null)
            {
                var endDateLocal = endDate.Value.ToLocalTime();
                PlainEndDate = endDateLocal.ToString("yyyy-MM-dd");
                PlainEndTime = endDateLocal.ToString("HH\\:mm");
                EndYear = endDateLocal.Year;
                EndMonth = endDateLocal.Month;
                EndDay = endDateLocal.Day;
            }
        }

        /// <summary>
        /// Constructor with separat time
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="startTime"></param>
        /// <param name="endDate"></param>
        /// <param name="endTime"></param>
        public Event(DateTime? startDate, TimeSpan? startTime, DateTime? endDate, TimeSpan? endTime) : this(startDate, endDate)
        {
            // Override start/end time 
            PlainStartTime = startTime?.ToString("hh\\:mm");
            PlainEndTime = endTime?.ToString("hh\\:mm");
            VerbatimEventDate = DwcFormatter.CreateDateIntervalString(startDate?.ToLocalTime(), startTime, endDate?.ToLocalTime(), endTime);
        }
        
        /// <summary>
        ///     An identifier for the set of information associated with an Event (something that occurs at a place and time). 
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        ///     Start date/time of the event in W. Europe Standard Time.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Divide year in 48 "weeks"
        /// </summary>
        public int? StartHistogramWeek => CalculateHistogramWeek(StartDate);

        /// <summary>
        /// Start year of the event, Swedish localization
        /// </summary>
        public int? StartYear { get; set; }

        /// <summary>
        /// Start month of the event, Swedish localization
        /// </summary>
        public int? StartMonth { get; set; }

        /// <summary>
        /// Start day of the event, Swedish localization
        /// </summary>
        public int? StartDay { get; set; }

        /// <summary>
        ///     End date/time of the event in W. Europe Standard Time.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Divide year in 48 weeks 
        /// </summary>
        public int? EndHistogramWeek => CalculateHistogramWeek(EndDate);

        /// <summary>
        /// End year of the event, Swedish localization
        /// </summary>
        public int? EndYear { get; set; }

        /// <summary>
        /// End month of the event, Swedish localization
        /// </summary>
        public int? EndMonth { get; set; }

        /// <summary>
        /// End day of the event, Swedish localization
        /// </summary>
        public int? EndDay { get; set; }

        /// <summary>
        ///     Start date of the event in the format yyyy-MM-dd.
        /// </summary>
        public string PlainStartDate { get; set; }

        /// <summary>
        ///     End date of the event in the format yyyy-MM-dd.
        /// </summary>
        public string PlainEndDate { get; set; }

        /// <summary>
        ///     Start time of the event in W. Europe Standard Time formatted as hh:mm.
        /// </summary>
        public string PlainStartTime { get; set; }

        /// <summary>
        ///     End time of the event in W. Europe Standard Time formatted as hh:mm. 
        /// </summary>
        public string PlainEndTime { get; set; }

        /// <summary>
        ///    DiscoveryMethod from Artportalen.
        /// </summary>
        public VocabularyValue DiscoveryMethod { get; set; }

        /// <summary>
        ///     Comments or notes about the Event.
        /// </summary>
        public string EventRemarks { get; set; }

        /// <summary>
        ///     One of a) an indicator of the existence of, b) a
        ///     reference to (publication, URI), or c) the text of
        ///     notes taken in the field about the Event.
        /// </summary>
        public string FieldNotes { get; set; }

        /// <summary>
        ///     An identifier given to the event in the field. Often
        ///     serves as a link between field notes and the Event.
        /// </summary>
        public string FieldNumber { get; set; }

        /// <summary>
        ///     A category or description of the habitat in which the Event occurred.
        /// </summary>
        public string Habitat { get; set; }

        /// <summary>
        ///     Multimedia associated with the event.
        /// </summary>
        public ICollection<Multimedia> Media { get; set; }

        /// <summary>
        ///     Measurement or facts associated with the event.
        /// </summary>
        public ICollection<ExtendedMeasurementOrFact> MeasurementOrFacts { get; set; }

        /// <summary>
        /// An identifier for the broader Event that groups this and potentially other Events.
        /// </summary>
        /// <example>
        ///     A1 (parentEventID to identify the main Whittaker Plot in nested samples, each with its own eventID - A1:1, A1:2).
        /// </example>
        public string ParentEventId { get; set; }

        /// <summary>
        ///     The amount of effort expended during an Event.
        /// </summary>
        public string SamplingEffort { get; set; }

        /// <summary>
        ///     The name of, reference to, or description of the
        ///     method or protocol used during an Event.
        /// </summary>
        public string SamplingProtocol { get; set; }

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
        ///     The verbatim original representation of the date and time information for an Event.
        ///     Examples: spring 1910, Marzo 2002, 1999-03-XX, 17IV1934.
        /// </summary>
        public string VerbatimEventDate { get; set; }

        /// <summary>
        /// Weather during event
        /// </summary>
        public Weather Weather { get; set; }
    }
}