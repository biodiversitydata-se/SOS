using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Models;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Factories
{
    public class KulObservationFactory : Interfaces.IKulObservationFactory
    {
        private readonly IKulObservationService _kulObservationService;
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private readonly ILogger<KulObservationFactory> _logger;
        private readonly KulServiceConfiguration _kulServiceConfiguration;

        public KulObservationFactory(
            IKulObservationService kulObservationService,
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            KulServiceConfiguration kulServiceConfiguration,
            ILogger<KulObservationFactory> logger)
        {
            _kulObservationService = kulObservationService;
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository;
            _kulServiceConfiguration = kulServiceConfiguration;
            _logger = logger;
        }

        private string GetKulHarvestSettingsInfoString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("KUL Harvest settings:");
            sb.AppendLine($"  Start Harvest Year: {_kulServiceConfiguration.StartHarvestYear}");
            sb.AppendLine($"  Max Number Of Sightings Harvested: {_kulServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
        
        public async Task<bool> HarvestObservationsAsync()
        {
            _logger.LogInformation("Start harvesting sightings for KUL data provider");
            _logger.LogInformation(GetKulHarvestSettingsInfoString());

            // Make sure we have an empty collection.
            _logger.LogInformation("Empty collection for KUL verbatim collection");
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
                var sightings = await _kulObservationService.GetAsync(changedFrom, changedFrom.AddYears(1));
                var aggregates = sightings.ToAggregates().ToArray();
                nrSightingsHarvested += aggregates.Length;

                // Add sightings to MongoDb
                await _kulObservationVerbatimRepository.AddManyAsync(aggregates);

                changedFrom = changedFrom.AddYears(1);
            }

            _logger.LogInformation("Finished harvesting sightings for KUL data provider");
            return true;
        }
    }
}
