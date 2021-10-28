using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Helpers
{
    /// <summary>
    ///     Class that can be used for converting UTC dates to local time.
    /// </summary>
    public static class LocalDateTimeConverterHelper
    {
        private static TimeZoneInfo swedenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        
        public static void ConvertToLocalTime(IEnumerable<Observation> processedObservations)
        {
            if (processedObservations == null) return;

            foreach (var observation in processedObservations)
            {
                ConvertToLocalTime(observation);
            }
        }

        public static void ConvertToLocalTime(IEnumerable<IDictionary<string, object>> processedRecords)
        {
            if (processedRecords == null) return;
            
            foreach (var record in processedRecords)
            {
                ConvertToLocalTime(record);
            }
        }

        public static void ConvertToLocalTime(Observation obs)
        {
            obs.Created = obs.Created.ToLocalTime();
            obs.Modified = obs.Modified?.ToLocalTime();
            if (obs.Event != null)
            {
                obs.Event.StartDate = obs.Event.StartDate?.ToLocalTime();
                obs.Event.EndDate = obs.Event.EndDate?.ToLocalTime();
            }

            if (obs.Occurrence != null)
            {
                obs.Occurrence.ReportedDate = obs.Occurrence.ReportedDate?.ToLocalTime();
            }
        }

        private static void ConvertToLocalTime(IDictionary<string, object> obs)
        {
            // Created
            if (obs.TryGetValue("created", out var createdDateObject))
            {
                if (createdDateObject is string createdDateString)
                {
                    obs["created"] = DateTime.Parse(createdDateString);
                }
            }

            // Modified
            if (obs.TryGetValue("modified", out var modifiedDateObject))
            {
                if (modifiedDateObject is string modifiedDateString)
                {
                    obs["modified"] = DateTime.Parse(modifiedDateString);
                }
            }

            // Event
            if (obs.TryGetValue("event", out var eventObject))
            {
                if (eventObject is IDictionary<string, object> eventDictionary)
                {
                    // StartDate
                    if (eventDictionary.TryGetValue("startDate", out var startDateObject))
                    {
                        if (startDateObject is string startDateString)
                        {
                            eventDictionary["startDate"] = DateTime.Parse(startDateString);
                        }
                    }

                    // EndDate
                    if (eventDictionary.TryGetValue("endDate", out var endDateObject))
                    {
                        if (endDateObject is string endDateString)
                        {
                            eventDictionary["endDate"] = DateTime.Parse(endDateString);
                        }
                    }
                }
            }

            // Occurrence
            if (obs.TryGetValue("occurrence", out var occurrenceObject))
            {
                if (occurrenceObject is IDictionary<string, object> occurrenceDictionary)
                {
                    // StartDate
                    if (occurrenceDictionary.TryGetValue("reportedDate", out var reportedDateObject))
                    {
                        if (reportedDateObject is string reportedDateString)
                        {
                            occurrenceDictionary["reportedDate"] = DateTime.Parse(reportedDateString);
                        }
                    }
                }
            }
        }
    }
}