using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Destination.Nors.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    public class NorsObservationHarvester : Interfaces.INorsObservationHarvester
    {
        private readonly INorsObservationService _norsObservationService;
        private readonly INorsObservationVerbatimRepository _norsObservationVerbatimRepository;
        private readonly ILogger<NorsObservationHarvester> _logger;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="norsObservationService"></param>
        /// <param name="norsObservationVerbatimRepository"></param>
        /// <param name="norsServiceConfiguration"></param>
        /// <param name="logger"></param>
        public NorsObservationHarvester(
            INorsObservationService norsObservationService,
            INorsObservationVerbatimRepository norsObservationVerbatimRepository,
            NorsServiceConfiguration norsServiceConfiguration,
            ILogger<NorsObservationHarvester> logger)
        {
            _norsObservationService = norsObservationService ?? throw new ArgumentNullException(nameof(norsObservationService));
            _norsObservationVerbatimRepository = norsObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(norsObservationVerbatimRepository));
            _norsServiceConfiguration = norsServiceConfiguration ?? throw new ArgumentNullException(nameof(norsServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetNorsHarvestSettingsInfoString()
        {
           var sb = new StringBuilder();
            sb.AppendLine("NORS Harvest settings:");
            sb.AppendLine($"  Page size: {_norsServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine($"  Max Number Of Sightings Harvested: {_norsServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
        
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken  cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(NorsObservationVerbatim), DataSet.NorsObservations, DateTime.Now);

            try
            {
                var start = DateTime.Now;
                _logger.LogInformation("Start harvesting sightings for NORS data provider");
                _logger.LogInformation(GetNorsHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for NORS verbatim collection");
                await _norsObservationVerbatimRepository.DeleteCollectionAsync();
                await _norsObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for NORS verbatim collection");

                var nrSightingsHarvested = 0;
                var result = await _norsObservationService.GetAsync(0);
                var maxId = result?.Item1 ?? 0;
                var sightings = result?.Item2;

                // Loop until all sightings are fetched.
                while (sightings?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    
                    var aggregates = sightings.ToVerbatims().ToArray();
                    nrSightingsHarvested += aggregates.Length;

                    // Add sightings to MongoDb
                    await _norsObservationVerbatimRepository.AddManyAsync(aggregates);

                    if (_norsServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _norsServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    result = await _norsObservationService.GetAsync(maxId);
                    maxId = result?.Item1 ?? 0;
                    sightings = result.Item2;
                }

                _logger.LogInformation("Finished harvesting sightings for NORS data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("NORS harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest NORS");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }
    }
}
