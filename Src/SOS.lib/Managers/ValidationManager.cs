using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Managers
{
    public class ValidationManager : IValidationManager
    {
        private readonly IInvalidObservationRepository _invalidObservationRepository;        
        private readonly ILogger<ValidationManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="invalidObservationRepository"></param>
        /// <param name="logger"></param>
        public ValidationManager(IInvalidObservationRepository invalidObservationRepository, ILogger<ValidationManager> logger)
        {
            _invalidObservationRepository = invalidObservationRepository ??
                                            throw new ArgumentNullException(nameof(invalidObservationRepository));       
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

        /// <summary>
        /// Checks if an observation is valid or not.
        /// </summary>
        /// <param name="observation"></param>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        public InvalidObservation ValidateObservation(Observation observation, DataProvider dataProvider)
        {
            var observationValidation = new InvalidObservation(observation.DataProviderId.ToString(), dataProvider.Names.Translate("en-GB"), observation.Occurrence.OccurrenceId);

            if (observation.Event?.StartDate == null || observation.Event.EndDate == null)
            {
                observationValidation.Defects.Add("Event StartDate and/or EndDate is missing");
            }

            if (observation.Taxon == null)
            {
                observationValidation.Defects.Add("Taxon not found");
            }

            if ((observation.Location?.CoordinateUncertaintyInMeters ?? 0) > 100000)
            {
                observationValidation.Defects.Add($"CoordinateUncertaintyInMeters exceeds max value 100 km ({observation.Location?.CoordinateUncertaintyInMeters ?? 0}m)");
            }

            if (observation.Location == null || !observation.Location.DecimalLatitude.HasValue ||
                !observation.Location.DecimalLongitude.HasValue)
            {
                observationValidation.Defects.Add("Coordinate is missing");
            }
            else if (!observation.IsInEconomicZoneOfSweden)
            {
                observationValidation.Defects.Add($"Sighting outside Swedish economic zone (lon: {observation.Location?.DecimalLongitude}, lat:{observation.Location?.DecimalLatitude})");
            }

            if (observation.Location?.Point == null)
            {
                observationValidation.Defects.Add("Location point is missing");
            }

            if (observation.Location?.PointLocation == null)
            {
                observationValidation.Defects.Add("Location point location is missing");
            }

            if (observation.Location?.PointWithBuffer == null)
            {
                observationValidation.Defects.Add("Location point with buffer is missing");
            }

            if (string.IsNullOrEmpty(observation?.Occurrence.CatalogNumber))
            {
                observationValidation.Defects.Add("CatalogNumber is missing");
            }

            return observationValidation;
        }

        /// <inheritdoc />
        public async Task VerifyCollectionAsync(JobRunModes mode)
        {
            _invalidObservationRepository.LiveMode = mode == JobRunModes.IncrementalActiveInstance;
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
    }
}
