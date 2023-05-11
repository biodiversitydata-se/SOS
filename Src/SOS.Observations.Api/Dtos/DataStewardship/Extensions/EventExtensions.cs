using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SOS.Observations.Api.Dtos.DataStewardship.Extensions
{
    public static class EventExtensions
    {
        /// <summary>
        /// Cast event model to csv
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this EventDto @event)
        {
            if (@event == null)
            {
                return null!;
            }

            return new[] { @event }.ToCsv();
        }

        /// <summary>
        /// Caste event models to csv
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public static byte[] ToCsv(this IEnumerable<EventDto> events)
        {
            if (!events?.Any() ?? true)
            {
                return null!;
            }

            using var stream = new MemoryStream();
            using var csvFileHelper = new CsvFileHelper();
            csvFileHelper.InitializeWrite(stream, "\t");
            csvFileHelper.WriteRow(new[] {
                "event id",
                "type",
                "parent id",
                "start date",
                "end date",
                "location protected",
                "survey location",
                "recorder code",
                "recorder organisation",
                "sampling protocol",
                "remarks",
                "no observations",
                "data set",
                "occurrence id's"
            });

            foreach (var evt in events)
            {
                csvFileHelper.WriteRow(new[] {
                    evt.EventID,
                    evt.EventType,
                    evt.ParentEventID,
                    evt.EventStartDate?.ToLongDateString(),
                    evt.EventEndDate?.ToLongDateString(),
                    evt.LocationProtected?.ToString(),
                    $"{evt.SurveyLocation?.LocationID}/{evt.SurveyLocation?.Locality}",
                    evt.RecorderCode?.Concat(),
                    evt.RecorderOrganisation?.Select(o => $"{o.OrganisationID}/{o.OrganisationCode}").Concat(),
                    evt.SamplingProtocol,
                    evt.EventRemarks,
                    evt.NoObservations?.ToString(),
                    evt.Dataset?.Identifier,
                    evt.OccurrenceIds?.Concat(10)
                });
            }

            csvFileHelper.Flush();
            stream.Position = 0;
            var csv = stream.ToArray();
            csvFileHelper.FinishWrite();

            return csv;
        }

        public static EventDto ToEventDto(this Lib.Models.Processed.Observation.Observation observation, IEnumerable<string> occurrenceIds, CoordinateSys responseCoordinateSystem)
        {
            if (observation == null) return null;
            var ev = new EventDto();
            ev.EventID = observation.Event.EventId;
            ev.ParentEventID = observation.Event.ParentEventId;
            ev.EventRemarks = observation.Event.EventRemarks;
            ev.AssociatedMedia = observation.Event.Media.ToDtos();
            ev.Dataset = new DatasetInfoDto
            {
                Identifier = observation.DataStewardship?.DatasetIdentifier
                //Title = // need to lookup this from ObservationDataset index or store this information in Observation/Event
            };

            ev.EventStartDate = observation.Event.StartDate;
            ev.EventEndDate = observation.Event.EndDate;
            ev.SamplingProtocol = observation.Event.SamplingProtocol;
            ev.SurveyLocation = observation.Location.ToDto(responseCoordinateSystem);
            //ev.LocationProtected = ?
            //ev.EventType = ?
            //ev.Weather = ?
            ev.RecorderCode = new List<string>
            {
                observation.Occurrence.RecordedBy
            };
            if (observation?.InstitutionCode?.Value != null || !string.IsNullOrEmpty(observation.InstitutionId))
            {
                ev.RecorderOrganisation = new List<OrganisationDto>
                {
                    new OrganisationDto
                    {
                        OrganisationID = observation?.InstitutionId,
                        OrganisationCode = observation?.InstitutionCode?.Value
                    }
                };
            }

            ev.OccurrenceIds = occurrenceIds?.ToList();
            ev.NoObservations = ev.OccurrenceIds == null || !ev.OccurrenceIds.Any();

            return ev;
        }

        public static EventDto ToEventDto(this Event observationEvent, CoordinateSys responseCoordinateSystem)
        {
            if (observationEvent == null) return null;

            var ev = new EventDto();
            ev.EventID = observationEvent.EventId;
            ev.ParentEventID = observationEvent.ParentEventId;
            ev.EventRemarks = observationEvent.EventRemarks;
            ev.AssociatedMedia = observationEvent.Media.ToDtos();
            ev.Dataset = observationEvent?.DataStewardship.ToDto();
            ev.EventStartDate = observationEvent.StartDate;
            ev.EventEndDate = observationEvent.EndDate;
            ev.SamplingProtocol = observationEvent.SamplingProtocol;
            ev.SurveyLocation = observationEvent?.Location?.ToDto(responseCoordinateSystem);
            ev.EventType = observationEvent?.EventType;
            ev.LocationProtected = observationEvent?.LocationProtected;
            ev.Weather = observationEvent?.Weather?.ToDto();
            ev.RecorderCode = observationEvent.RecorderCode;
            ev.RecorderOrganisation = observationEvent?.RecorderOrganisation?.Select(m => m.ToDto()).ToList();
            ev.OccurrenceIds = observationEvent?.OccurrenceIds;
            ev.NoObservations = ev.OccurrenceIds == null || !ev.OccurrenceIds.Any();

            return ev;
        }
    }
}
