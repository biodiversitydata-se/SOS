using Microsoft.AspNetCore.Http.HttpResults;
using SOS.Lib.Models.Processed.Observation;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

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

        public static void ConvertToLocalTime(IEnumerable<JsonNode> processedRecords)
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

        private static void ConvertToLocalTime(JsonNode obs)
        {
            // Created
            var createdDateString = (string)obs["created"];
            if (createdDateString != null)
            {
                obs["created"] = DateTime.Parse(createdDateString);
            }

            // Modified
            var modifiedDateString = (string)obs["modified"];
            if (modifiedDateString != null)
            {
                obs["modified"] = DateTime.Parse(modifiedDateString);
            }

            // Event
            var eventObject = obs["event"];
            if (eventObject != null)
            {
                // StartDate
                var startDateString = (string)eventObject["startDate"];
                if (startDateString != null)
                {
                    eventObject["startDate"] = DateTime.Parse(startDateString);
                }

                // EndDate
                var endDateString = (string)eventObject["endDate"];
                if (endDateString != null)
                {
                    eventObject["endDate"] = DateTime.Parse(endDateString);
                }
            }

            // Occurrence
            var occurrenceObject = obs["occurrence"];
            if (occurrenceObject != null)
            {
                // StartDate
                var reportedDateString = (string)occurrenceObject["reportedDate"];
                if (reportedDateString != null)
                {
                    occurrenceObject["reportedDate"] = DateTime.Parse(reportedDateString);
                }
            }
        }
    }
}