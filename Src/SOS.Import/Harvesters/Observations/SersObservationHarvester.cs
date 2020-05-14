using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Destination.Sers.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    public class SersObservationHarvester : Interfaces.ISersObservationHarvester
    {
        private readonly ISersObservationService _sersObservationService;
        private readonly ISersObservationVerbatimRepository _sersObservationVerbatimRepository;
        private readonly ILogger<SersObservationHarvester> _logger;
        private readonly SersServiceConfiguration _sersServiceConfiguration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sersObservationService"></param>
        /// <param name="sersObservationVerbatimRepository"></param>
        /// <param name="sersServiceConfiguration"></param>
        /// <param name="logger"></param>
        public SersObservationHarvester(
            ISersObservationService sersObservationService,
            ISersObservationVerbatimRepository sersObservationVerbatimRepository,
            SersServiceConfiguration sersServiceConfiguration,
            ILogger<SersObservationHarvester> logger)
        {
            _sersObservationService = sersObservationService ?? throw new ArgumentNullException(nameof(sersObservationService));
            _sersObservationVerbatimRepository = sersObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(sersObservationVerbatimRepository));
            _sersServiceConfiguration = sersServiceConfiguration ?? throw new ArgumentNullException(nameof(sersServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetSersHarvestSettingsInfoString()
        {
           var sb = new StringBuilder();
            sb.AppendLine("SERS Harvest settings:");
            sb.AppendLine($"  Page size: {_sersServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine($"  Max Number Of Sightings Harvested: {_sersServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
        
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken  cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(SersObservationVerbatim), DataSet.SersObservations, DateTime.Now);

            try
            {
                var start = DateTime.Now;
                _logger.LogInformation("Start harvesting sightings for SERS data provider");
                _logger.LogInformation(GetSersHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for SERS verbatim collection");
                await _sersObservationVerbatimRepository.DeleteCollectionAsync();
                await _sersObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for SERS verbatim collection");

                var nrSightingsHarvested = 0;
                var result = await _sersObservationService.GetAsync(0);
                var maxId = result?.Item1 ?? 0;

                // Loop until all sightings are fetched.
                while (maxId != 0)
                {
                    var sightings = result?.Item2;

                    cancellationToken?.ThrowIfCancellationRequested();
                    
                    var aggregates = sightings.ToVerbatims().ToArray();
                    nrSightingsHarvested += aggregates.Length;

                    // Add sightings to MongoDb
                    await _sersObservationVerbatimRepository.AddManyAsync(aggregates);

                    if (_sersServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _sersServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    result = await _sersObservationService.GetAsync(maxId + 1);
                    maxId = result?.Item1 ?? 0;
                }

                _logger.LogInformation("Finished harvesting sightings for SERS data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("SERS harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest SERS");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }
    }
}
