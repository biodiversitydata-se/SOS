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

namespace SOS.Import.Factories
{
    public class KulSightingFactory : Interfaces.IKulSightingFactory
    {
        private readonly IKulSightingRepository _kulSightingRepository;
        private readonly IKulSightingVerbatimRepository _kulSightingVerbatimRepository;
        private readonly ILogger<KulSightingFactory> _logger;

        public KulSightingFactory(
            IKulSightingRepository kulSightingRepository,
            IKulSightingVerbatimRepository kulSightingVerbatimRepository,
            ILogger<KulSightingFactory> logger)
        {
            _kulSightingRepository = kulSightingRepository;
            _kulSightingVerbatimRepository = kulSightingVerbatimRepository;
            _logger = logger;
        }

        public async Task<bool> AggregateAsync()
        {
            return await AggregateAsync(new KulAggregationOptions());
        }

        public async Task<bool> AggregateAsync(KulAggregationOptions options)
        {
            _logger.LogDebug("Start harvesting sightings for KUL data provider");
            
            // Make sure we have an empty collection.
            _logger.LogDebug("Empty collection for KUL verbatim collection");
            await _kulSightingVerbatimRepository.DeleteCollectionAsync();
            await _kulSightingVerbatimRepository.AddCollectionAsync();

            DateTime changedFrom = new DateTime(options.StartHarvestYear, 1, 1);
            DateTime changedToEnd = DateTime.Now;
            int nrSightingsHarvested = 0;

            // Loop until all sightings are fetched.
            while (changedFrom < changedToEnd)
            {
                if (options.MaxNumberOfSightingsHarvested.HasValue &&
                    nrSightingsHarvested >= options.MaxNumberOfSightingsHarvested)
                {
                    break;
                }

                // Get sightings for one year
                var sightings = await _kulSightingRepository.GetAsync(changedFrom, changedFrom.AddYears(1));
                var aggregates = sightings.ToAggregates().ToArray();
                nrSightingsHarvested += aggregates.Length;

                // Add sightings to MongoDb
                await _kulSightingVerbatimRepository.AddManyAsync(aggregates);

                changedFrom = changedFrom.AddYears(1);
            }

            _logger.LogDebug("Finished harvesting sightings for KUL data provider");
            return true;
        }
    }
}
