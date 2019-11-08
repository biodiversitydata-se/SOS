using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Models;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Repositories.Source.Kul.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Factories
{
    public class KulObservationFactory : Interfaces.IKulObservationFactory
    {
        private readonly IKulObservationRepository _kulObservationRepository;
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private readonly ILogger<KulObservationFactory> _logger;
        private readonly KulServiceConfiguration _kulServiceConfiguration;

        public KulObservationFactory(
            IKulObservationRepository kulObservationRepository,
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            KulServiceConfiguration kulServiceConfiguration,
            ILogger<KulObservationFactory> logger)
        {
            _kulObservationRepository = kulObservationRepository;
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository;
            _kulServiceConfiguration = kulServiceConfiguration;
            _logger = logger;
        }

        
        public async Task<bool> HarvestObservationsAsync()
        {
            _logger.LogDebug("Start harvesting sightings for KUL data provider");
            
            // Make sure we have an empty collection.
            _logger.LogDebug("Empty collection for KUL verbatim collection");
            await _kulObservationVerbatimRepository.DeleteCollectionAsync();
            await _kulObservationVerbatimRepository.AddCollectionAsync();

            DateTime changedFrom = new DateTime(_kulServiceConfiguration.StartHarvestYear, 1, 1);
            DateTime changedToEnd = DateTime.Now;
            int nrSightingsHarvested = 0;

            // Loop until all sightings are fetched.
            while (changedFrom < changedToEnd)
            {
                if (_kulServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                    nrSightingsHarvested >= _kulServiceConfiguration.MaxNumberOfSightingsHarvested)
                {
                    break;
                }

                // Get sightings for one year
                var sightings = await _kulObservationRepository.GetAsync(changedFrom, changedFrom.AddYears(1));
                var aggregates = sightings.ToAggregates().ToArray();
                nrSightingsHarvested += aggregates.Length;

                // Add sightings to MongoDb
                await _kulObservationVerbatimRepository.AddManyAsync(aggregates);

                changedFrom = changedFrom.AddYears(1);
            }

            _logger.LogDebug("Finished harvesting sightings for KUL data provider");
            return true;
        }
    }
}
