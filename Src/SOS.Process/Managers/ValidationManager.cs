using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Managers
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
        public ICollection<InvalidObservation> ValidateObservations(ref ICollection<ProcessedObservation> items)
        {
            var validItems = new List<ProcessedObservation>();
            var invalidItems = new List<InvalidObservation>();

            foreach (var item in items)
            {
                var invalidObservation =
                    new InvalidObservation(item.DatasetId, item.DatasetName, item.Occurrence.OccurrenceId);

                if (item.Taxon == null)
                {
                    invalidObservation.Defects.Add("Taxon not found");
                }

                if ((item.Location?.CoordinateUncertaintyInMeters ?? 0) > 100000)
                {
                    invalidObservation.Defects.Add("CoordinateUncertaintyInMeters exceeds max value 100 km");
                }

                if (!item.IsInEconomicZoneOfSweden)
                {
                    invalidObservation.Defects.Add("Sighting outside Swedish economic zone");
                }

                if (string.IsNullOrEmpty(item?.Occurrence.CatalogNumber))
                {
                    invalidObservation.Defects.Add("CatalogNumber is missing");
                }

                if (invalidObservation.Defects.Any())
                {
                    invalidItems.Add(invalidObservation);
                }
                else
                {
                    validItems.Add(item);
                }
            }

            items = validItems;

            return invalidItems.Any() ? invalidItems : null;
        }

        /// <inheritdoc />
        public async Task VerifyCollectionAsync(bool incrementalMode)
        {
            var collectionCreated = true;
            if (incrementalMode)
            {
                collectionCreated = await _invalidObservationRepository.VerifyCollectionAsync();
            }
            else
            {
                // Make sure invalid collection is empty 
                await _invalidObservationRepository.DeleteCollectionAsync();
                await _invalidObservationRepository.AddCollectionAsync();
            }

            if (collectionCreated)
            {
                await _invalidObservationRepository.CreateIndexAsync();
            }
        }
    }
}
