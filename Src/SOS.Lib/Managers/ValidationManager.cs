﻿using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Managers
{
    public class ValidationManager : IValidationManager
    {
        private readonly IInvalidObservationRepository _invalidObservationRepository;
        private readonly IInvalidEventRepository _invalidEventRepository;
        private readonly ILogger<ValidationManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>        
        public ValidationManager(IInvalidObservationRepository invalidObservationRepository,
            IInvalidEventRepository invalidEventRepository,
            ILogger<ValidationManager> logger)
        {
            _invalidObservationRepository = invalidObservationRepository ??
                                            throw new ArgumentNullException(nameof(invalidObservationRepository));
            _invalidEventRepository = invalidEventRepository ??
                                            throw new ArgumentNullException(nameof(invalidEventRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> AddInvalidObservationsToDb(ICollection<InvalidObservation> invalidObservations)
        {
            try
            {
                if (invalidObservations == null || invalidObservations.Count == 0) return false;

                await _invalidObservationRepository.AddManyAsync(invalidObservations);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Write invalid observations failed");
                return false;
            }
        }

        /// <inheritdoc />
        public ICollection<InvalidObservation> ValidateObservations(ref ICollection<Observation> observations, DataProvider dataProvider)
        {
            var validItems = new List<Observation>();
            var invalidItems = new List<InvalidObservation>();
            foreach (var observation in observations)
            {
                var observationValidation = ValidateObservation(observation, dataProvider);

                if (observationValidation.IsInvalid)
                {
                    invalidItems.Add(observationValidation);
                }
                else
                {
                    validItems.Add(observation);
                }
            }

            observations = validItems;
            return invalidItems.Any() ? invalidItems : null;
        }

        private void AddError(
            Dictionary<ObservationDefect.ObservationDefectType, List<string>> errors,
            ObservationDefect.ObservationDefectType errorType,
            string errorDescription)
        {
            if (!errors.ContainsKey(errorType))
            {
                errors.Add(errorType, new List<string>());
            }

            errors[errorType].Add(errorDescription);
        }

        /// <summary>
        /// Checks if an observation is valid or not.
        /// </summary>
        /// <param name="observation"></param>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        public InvalidObservation ValidateObservation(Observation observation, DataProvider dataProvider)
        {
            var observationValidation = new InvalidObservation(observation.DataProviderId.ToString(), dataProvider.Names.Translate("en-GB"), observation.Occurrence.OccurrenceId);
            var errors = new Dictionary<ObservationDefect.ObservationDefectType, List<string>>();

            if (observation.Modified.HasValue && observation.Modified.Value > DateTime.UtcNow)
            {
                AddError(errors, ObservationDefect.ObservationDefectType.DateError, $"Modified date '{observation.Modified.Value}' is in the future");
            }

            if (observation.Event?.StartDate == null || observation.Event.EndDate == null)
            {
                AddError(errors, ObservationDefect.ObservationDefectType.DateError, "Event StartDate and/or EndDate is missing");
            }
            else if (observation.Event.StartDate > observation.Event.EndDate)
            {                
                AddError(errors, ObservationDefect.ObservationDefectType.DateError, $"Event StartDate '{observation.Event.StartDate}' is greater than EndDate '{observation.Event.EndDate}'");
            }

            // Many observations in MVM, VirtualHerbarium, SHARK, ObservationDatabase doesn't have any RecordedBy.
            //if (string.IsNullOrEmpty(observation.Occurrence.RecordedBy))
            //{
            //    AddError(errors, ObservationDefect.ObservationDefectType.RecordedByError, "RecordedBy is null or empty");
            //}
            if (!string.IsNullOrEmpty(observation.Occurrence.RecordedBy) && observation.Occurrence.RecordedBy.Equals("[SuspendedUser]", StringComparison.InvariantCultureIgnoreCase))
            {
                AddError(errors, ObservationDefect.ObservationDefectType.RecordedByError, "RecordedBy user is suspended");
            }

            if ((observation.Taxon?.Id ?? -1) == -1)
            {
                string taxonError = null;
                if (!string.IsNullOrEmpty(observation.Taxon?.VerbatimId) && observation.Taxon?.VerbatimId != (-1).ToString())
                {
                    taxonError = $"TaxonId={observation.Taxon?.VerbatimId}";
                }
                
                if (!string.IsNullOrEmpty(observation.Taxon?.VerbatimName))
                {
                    if (taxonError != null)
                        taxonError += ", Names=" + observation.Taxon?.VerbatimName;
                    else
                        taxonError = "Names=" + observation.Taxon?.VerbatimName;
                }

                if (taxonError == null)
                    taxonError = "Taxon information is missing";

                AddError(errors, ObservationDefect.ObservationDefectType.TaxonNotFound, taxonError);
            }

            if (observation.Location == null || !observation.Location.DecimalLatitude.HasValue ||
                !observation.Location.DecimalLongitude.HasValue)
            {
                AddError(errors, ObservationDefect.ObservationDefectType.GeographicsError, "Coordinates are missing");
            }
            else if (!observation.Location.IsInEconomicZoneOfSweden)
            {
                AddError(errors, ObservationDefect.ObservationDefectType.LocationOutsideOfSweden, $"Sighting outside Swedish economic zone (lon: {observation.Location?.DecimalLongitude}, lat:{observation.Location?.DecimalLatitude})");
            }
            else if (observation.Location?.Point == null || observation.Location?.PointLocation == null || observation.Location?.PointWithBuffer == null)
            {
                AddError(errors, ObservationDefect.ObservationDefectType.GeographicsError, "Location point is missing");
            }

            if (string.IsNullOrEmpty(observation?.Occurrence.CatalogNumber))
            {
                AddError(errors, ObservationDefect.ObservationDefectType.IdentifierError, "CatalogNumber is missing");
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors) 
                {
                    observationValidation.Defects.Add(new ObservationDefect(error.Key, string.Join(", ", error.Value)));
                }
            }

            return observationValidation;
        }

        /// <inheritdoc />
        public async Task VerifyCollectionAsync(JobRunModes mode)
        {
            var collectionCreated = false;
            if (mode == JobRunModes.Full)
            {
                // Make sure invalid collection is empty 
                await _invalidObservationRepository.DeleteCollectionAsync();
                await _invalidObservationRepository.AddCollectionAsync();
            }
            else
            {
                collectionCreated = await _invalidObservationRepository.VerifyCollectionAsync();
            }

            if (collectionCreated)
            {
                await _invalidObservationRepository.CreateIndexAsync();
            }
        }

        public async Task<bool> AddInvalidEventsToDb(ICollection<InvalidEvent> invalidEvents)
        {
            try
            {
                if (invalidEvents == null || invalidEvents.Count == 0) return false;

                await _invalidEventRepository.AddManyAsync(invalidEvents);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Write invalid events failed");
                return false;
            }
        }

        public ICollection<InvalidEvent> ValidateEvents(ref ICollection<Models.Processed.DataStewardship.Event.Event> events, DataProvider dataProvider)
        {
            var validItems = new List<Models.Processed.DataStewardship.Event.Event>();
            var invalidItems = new List<InvalidEvent>();
            foreach (var ev in events)
            {
                var eventValidation = ValidateEvent(ev, dataProvider);

                if (eventValidation.IsInvalid)
                {
                    invalidItems.Add(eventValidation);
                }
                else
                {
                    validItems.Add(ev);
                }
            }

            events = validItems;
            return invalidItems.Any() ? invalidItems : null;
        }

        public InvalidEvent ValidateEvent(Models.Processed.DataStewardship.Event.Event ev, DataProvider dataProvider)
        {
            var eventValidation = new InvalidEvent(ev.DataProviderId.ToString(), dataProvider.Names.Translate("en-GB"), ev.EventId);

            if (ev.StartDate == null || ev.EndDate == null)
            {
                eventValidation.Defects.Add(new EventDefect(
                    EventDefect.EventDefectType.MissingMandatoryField,
                    "Event StartDate and/or EndDate is missing")
                );
            }
            else
            {
                if (ev.StartDate > ev.EndDate)
                {
                    eventValidation.Defects.Add(new EventDefect(
                        EventDefect.EventDefectType.LogicError,
                        "Event StartDate is greater than EndDate")
                    );
                }
            }


            if ((ev.Location?.CoordinateUncertaintyInMeters ?? 0) > 100000)
            {
                eventValidation.Defects.Add(new EventDefect(
                    EventDefect.EventDefectType.ValueOutOfRange,
                    $"CoordinateUncertaintyInMeters exceeds max value 100 km ({ev.Location?.CoordinateUncertaintyInMeters ?? 0}m)")
                );
            }

            if (ev.Location == null || !ev.Location.DecimalLatitude.HasValue ||
                !ev.Location.DecimalLongitude.HasValue)
            {
                eventValidation.Defects.Add(new EventDefect(EventDefect.EventDefectType.MissingMandatoryField, "Coordinates are missing")
                );
            }
            else if (!ev.Location.IsInEconomicZoneOfSweden)
            {
                eventValidation.Defects.Add(new EventDefect(
                    EventDefect.EventDefectType.LocationOutsideOfSweden,
                    $"Sighting outside Swedish economic zone (lon: {ev.Location?.DecimalLongitude}, lat:{ev.Location?.DecimalLatitude})")
                );
            }

            if (ev.Location?.Point == null)
            {
                eventValidation.Defects.Add(new EventDefect(
                    EventDefect.EventDefectType.MissingMandatoryField,
                    "Location point is missing")
                );
            }

            if (ev.Location?.PointLocation == null)
            {
                eventValidation.Defects.Add(new EventDefect(EventDefect.EventDefectType.MissingMandatoryField, "Point location is missing")
                );
            }

            if (ev.Location?.PointWithBuffer == null)
            {
                eventValidation.Defects.Add(new EventDefect(EventDefect.EventDefectType.MissingMandatoryField, "Location point with buffer is missing")
                );
            }

            return eventValidation;
        }

        public async Task VerifyEventCollectionAsync(JobRunModes mode)
        {
            var collectionCreated = false;
            if (mode == JobRunModes.Full)
            {
                // Make sure invalid collection is empty 
                await _invalidEventRepository.DeleteCollectionAsync();
                await _invalidEventRepository.AddCollectionAsync();
            }
            else
            {
                collectionCreated = await _invalidEventRepository.VerifyCollectionAsync();
            }

            if (collectionCreated)
            {
                await _invalidEventRepository.CreateIndexAsync();
            }
        }
    }
}