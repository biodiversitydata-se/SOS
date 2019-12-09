using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Import.Factories
{
    public class KulObservationFactory : Interfaces.IKulObservationFactory
    {
        private readonly IKulObservationService _kulObservationService;
        private readonly IKulObservationVerbatimRepository _kulObservationVerbatimRepository;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<KulObservationFactory> _logger;
        private readonly KulServiceConfiguration _kulServiceConfiguration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kulObservationService"></param>
        /// <param name="kulObservationVerbatimRepository"></param>
        /// <param name="kulServiceConfiguration"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public KulObservationFactory(
            IKulObservationService kulObservationService,
            IKulObservationVerbatimRepository kulObservationVerbatimRepository,
            KulServiceConfiguration kulServiceConfiguration,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<KulObservationFactory> logger)
        {
            _kulObservationService = kulObservationService;
            _kulObservationVerbatimRepository = kulObservationVerbatimRepository;
            _kulServiceConfiguration = kulServiceConfiguration;
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
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
        
        public async Task<bool> HarvestObservationsAsync(IJobCancellationToken  cancellationToken)
        {
            try
            {
                var start = DateTime.Now;
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
                    cancellationToken?.ThrowIfCancellationRequested();
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
                
                // Update harvest info
                return await _harvestInfoRepository.UpdateHarvestInfoAsync(
                    nameof(KulObservationVerbatim),
                    DataProvider.KUL,
                    start,
                    DateTime.Now, 
                    nrSightingsHarvested);
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("KUL harvest was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest KUL");
                throw;
            }
        }
    }
}
