using SOS.Lib.Models.Processed.DataStewardship.Common;
using SOS.Lib.Models.Processed.DataStewardship.Event;
using SOS.Lib.Models.Processed.Observation;
using System;
using System.Collections.Generic;
using System.Linq;
using Event = SOS.Lib.Models.Processed.DataStewardship.Event.Event;

namespace SOS.Lib.Extensions
{
    public static class ObservationEventExtensions
    {
        public static Event ToEvent(this Observation observation, IEnumerable<string> occurrenceIds, int dataProviderId)
        {
            if (observation == null) return null;
            
            var ev = new Event();
            ev.EventId = observation.Event.EventId;
            ev.ParentEventId = observation.Event.ParentEventId;
            ev.EventRemarks = observation.Event.EventRemarks;
            ev.Media = observation.Event.Media;
            //ev.AssociatedMedia = observation.Event.Media.ToAssociatedMedias(); // todo
            ev.DataStewardship = new DataStewardshipInfo
            {
                DatasetIdentifier = observation.DataStewardship?.DatasetIdentifier
                //Title = // need to lookup this from Dataset index or store this information in Observation/Event
            };

            ev.Created = DateTime.Now;
            ev.DataProviderId = dataProviderId;
            ev.StartDate = observation.Event.StartDate;
            ev.EndDate = observation.Event.EndDate;
            ev.SamplingProtocol = observation.Event.SamplingProtocol;
            ev.SamplingEffort = observation.Event.SamplingEffort;
            ev.SampleSizeValue = observation.Event.SampleSizeValue;
            ev.SampleSizeUnit = observation.Event.SampleSizeUnit;
            ev.DiscoveryMethod = observation.Event.DiscoveryMethod;
            ev.MeasurementOrFacts = observation.Event.MeasurementOrFacts;
            ev.Habitat = observation.Event.Habitat;            
            ev.Location = observation.Location;
            //ev.LocationProtected = ?
            //ev.EventType = ?
            //ev.Weather = ?
            ev.RecorderCode = new List<string>
            {
                observation.Occurrence.RecordedBy
            };
            if (observation?.InstitutionCode?.Value != null || !string.IsNullOrEmpty(observation.InstitutionId))
            {
                ev.RecorderOrganisation = new List<Organisation>
                {
                    new Organisation
                    {
                        OrganisationID = observation?.InstitutionId,
                        OrganisationCode = observation?.InstitutionCode?.Value
                    }
                };
            }

            ev.OccurrenceIds = occurrenceIds.ToList();
            ev.NoObservations = !ev.OccurrenceIds.Any();
            
            return ev;
        }
    }
}
