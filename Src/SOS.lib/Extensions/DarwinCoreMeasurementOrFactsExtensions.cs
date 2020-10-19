using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Lib.Extensions
{
    /// <summary>
    ///     Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreMeasurementOrFactsExtensions
    {
        public static ExtendedMeasurementOrFact ToProcessedExtendedMeasurementOrFact(this DwcMeasurementOrFact dwcMeasurementOrFact)
        {
            return new ExtendedMeasurementOrFact
            {
                MeasurementID = dwcMeasurementOrFact.MeasurementID,
                MeasurementType = dwcMeasurementOrFact.MeasurementType,
                MeasurementValue = dwcMeasurementOrFact.MeasurementValue,
                MeasurementAccuracy = dwcMeasurementOrFact.MeasurementAccuracy,
                MeasurementUnit = dwcMeasurementOrFact.MeasurementUnit,
                MeasurementDeterminedDate = dwcMeasurementOrFact.MeasurementDeterminedDate,
                MeasurementDeterminedBy = dwcMeasurementOrFact.MeasurementDeterminedBy,
                MeasurementMethod = dwcMeasurementOrFact.MeasurementMethod,
                MeasurementRemarks = dwcMeasurementOrFact.MeasurementRemarks
            };
        }

        public static ExtendedMeasurementOrFact ToProcessedExtendedMeasurementOrFact(this DwcExtendedMeasurementOrFact dwcEmof)
        {
            return new ExtendedMeasurementOrFact()
            {
                MeasurementID = dwcEmof.MeasurementID,
                MeasurementType = dwcEmof.MeasurementType,
                MeasurementTypeID = dwcEmof.MeasurementTypeID,
                MeasurementValue = dwcEmof.MeasurementValue,
                MeasurementValueID = dwcEmof.MeasurementValueID,
                MeasurementAccuracy = dwcEmof.MeasurementAccuracy,
                MeasurementUnit = dwcEmof.MeasurementUnit,
                MeasurementUnitID = dwcEmof.MeasurementUnitID,
                MeasurementDeterminedDate = dwcEmof.MeasurementDeterminedDate,
                MeasurementDeterminedBy = dwcEmof.MeasurementDeterminedBy,
                MeasurementMethod = dwcEmof.MeasurementMethod,
                MeasurementRemarks = dwcEmof.MeasurementRemarks,
                OccurrenceID = dwcEmof.OccurrenceID
            };
        }

        public static IEnumerable<ExtendedMeasurementOrFactRow> ToExtendedMeasurementOrFactRows(this
            IEnumerable<Observation> processedObservations)
        {
            return processedObservations?.SelectMany(ToExtendedMeasurementOrFactRows);
        }

        public static IEnumerable<ExtendedMeasurementOrFactRow> ToExtendedMeasurementOrFactRows(this
            Observation observation)
        {
            IEnumerable<ExtendedMeasurementOrFactRow> occurrenceEmof = null;
            IEnumerable<ExtendedMeasurementOrFactRow> eventEmof = null;
            IEnumerable<ExtendedMeasurementOrFactRow> projectEmof = null;
            if (observation.MeasurementOrFacts != null)
            {
                occurrenceEmof = observation.MeasurementOrFacts.Select(m => m.ToExtendedMeasurementOrFactRow());
            }

            if (observation.Event.MeasurementOrFacts != null)
            {
                eventEmof = observation.Event.MeasurementOrFacts.Select(m => m.ToExtendedMeasurementOrFactRow(observation.Event.EventId));
            }

            if (observation.Projects != null)
            {
                projectEmof = observation.Projects.SelectMany(project =>
                    ToExtendedMeasurementOrFactRows(project, observation.Occurrence.OccurrenceId));
            }

            return (occurrenceEmof ?? Enumerable.Empty<ExtendedMeasurementOrFactRow>())
                .Union(eventEmof ?? Enumerable.Empty<ExtendedMeasurementOrFactRow>())
                .Union(projectEmof ?? Enumerable.Empty<ExtendedMeasurementOrFactRow>());
        }

        public static IEnumerable<ExtendedMeasurementOrFactRow> ToExtendedMeasurementOrFactRows(this
            IEnumerable<Project> projects, string occurrenceId)
        {
            return projects?.SelectMany(project => ToExtendedMeasurementOrFactRows(project, occurrenceId));
        }

        private static IEnumerable<ExtendedMeasurementOrFactRow> ToExtendedMeasurementOrFactRows(
            Project project, string occurrenceId)
        {
            if (project?.ProjectParameters == null || !project.ProjectParameters.Any())
            {
                return Enumerable.Empty<ExtendedMeasurementOrFactRow>();
            }

            var rows = project?.ProjectParameters?.Select(projectParameter =>
                ToExtendedMeasurementOrFactRow(occurrenceId, project, projectParameter));
            return rows;
        }

        private static string GetMeasurementMethodDescription(Project project)
        {
            if (string.IsNullOrEmpty(project.SurveyMethod) && string.IsNullOrEmpty(project.SurveyMethodUrl))
            {
                return null;
            }

            if (string.IsNullOrEmpty(project.SurveyMethodUrl))
            {
                return project.SurveyMethod;
            }

            if (string.IsNullOrEmpty(project.SurveyMethod))
            {
                return project.SurveyMethodUrl;
            }

            return $"{project.SurveyMethod} [{project.SurveyMethodUrl}]";
        }

        private static ExtendedMeasurementOrFactRow ToExtendedMeasurementOrFactRow(
            string occurrenceId,
            Project project,
            ProjectParameter projectParameter)
        {
            var row = new ExtendedMeasurementOrFactRow();
            row.OccurrenceID = occurrenceId;
            row.MeasurementID = project.Id; // Should this be ProjectId or ProjectParameterId?
            //row.MeasurementID = projectParameter.ProjectParameterId.ToString(); // Should this be ProjectId or ProjectParameterId?
            row.MeasurementType = projectParameter.Name;
            row.MeasurementValue = projectParameter.Value;
            row.MeasurementUnit = projectParameter.Unit;
            row.MeasurementDeterminedDate = DwcFormatter.CreateDateIntervalString(project.StartDate, project.EndDate);
            row.MeasurementMethod = GetMeasurementMethodDescription(project);
            row.MeasurementRemarks = projectParameter.Description;

            //row.MeasurementAccuracy = ?
            //row.MeasurementDeterminedBy = ?
            //row.MeasurementRemarks = ?
            //row.MeasurementTypeID = ?
            //row.MeasurementUnitID = ?
            //row.MeasurementValueID = ?
            return row;
        }

        private static ExtendedMeasurementOrFactRow ToExtendedMeasurementOrFactRow(
            this ExtendedMeasurementOrFact processedEmof, string eventId = null)
        {
            return new ExtendedMeasurementOrFactRow
            {
                MeasurementAccuracy = processedEmof.MeasurementAccuracy,
                MeasurementDeterminedBy = processedEmof.MeasurementDeterminedBy,
                MeasurementDeterminedDate = processedEmof.MeasurementDeterminedDate,
                MeasurementID = processedEmof.MeasurementID,
                MeasurementMethod = processedEmof.MeasurementMethod,
                MeasurementRemarks = !string.IsNullOrEmpty(eventId) ? ($"Measurement for EventID \"{eventId}\". " + processedEmof.MeasurementRemarks).TrimEnd() : processedEmof.MeasurementRemarks,
                MeasurementType = processedEmof.MeasurementType,
                MeasurementTypeID = processedEmof.MeasurementTypeID,
                MeasurementUnit = processedEmof.MeasurementUnit,
                MeasurementUnitID = processedEmof.MeasurementUnitID,
                MeasurementValue = processedEmof.MeasurementValue,
                MeasurementValueID = processedEmof.MeasurementValueID,
                OccurrenceID = processedEmof.OccurrenceID
            };
        }
    }
}