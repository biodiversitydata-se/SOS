using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Destination.Mvm.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    public class MvmObservationHarvester : Interfaces.IMvmObservationHarvester
    {
        private readonly IMvmObservationService _mvmObservationService;
        private readonly IMvmObservationVerbatimRepository _mvmObservationVerbatimRepository;
        private readonly ILogger<MvmObservationHarvester> _logger;
        private readonly MvmServiceConfiguration _mvmServiceConfiguration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mvmObservationService"></param>
        /// <param name="mvmObservationVerbatimRepository"></param>
        /// <param name="mvmServiceConfiguration"></param>
        /// <param name="logger"></param>
        public MvmObservationHarvester(
            IMvmObservationService mvmObservationService,
            IMvmObservationVerbatimRepository mvmObservationVerbatimRepository,
            MvmServiceConfiguration mvmServiceConfiguration,
            ILogger<MvmObservationHarvester> logger)
        {
            _mvmObservationService = mvmObservationService ?? throw new ArgumentNullException(nameof(mvmObservationService));
            _mvmObservationVerbatimRepository = mvmObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(mvmObservationVerbatimRepository));
            _mvmServiceConfiguration = mvmServiceConfiguration ?? throw new ArgumentNullException(nameof(mvmServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetMvmHarvestSettingsInfoString()
        {
           var sb = new StringBuilder();
            sb.AppendLine("MVM Harvest settings:");
            sb.AppendLine($"  Page size: {_mvmServiceConfiguration.MaxReturnedChangesInOnePage}");
            sb.AppendLine($"  Max Number Of Sightings Harvested: {_mvmServiceConfiguration.MaxNumberOfSightingsHarvested}");
            return sb.ToString();
        }
        
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken  cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(MvmObservationVerbatim), DataProviderType.MvmObservations, DateTime.Now);

            try
            {
                var start = DateTime.Now;
                _logger.LogInformation("Start harvesting sightings for MVM data provider");
                _logger.LogInformation(GetMvmHarvestSettingsInfoString());

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for MVM verbatim collection");
                await _mvmObservationVerbatimRepository.DeleteCollectionAsync();
                await _mvmObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for MVM verbatim collection");

                var nrSightingsHarvested = 0;
                var result = await _mvmObservationService.GetAsync(0);
                var maxId = result?.Item1 ?? 0;
                var dataLastModified = DateTime.MinValue;

                // Loop until all sightings are fetched.
                while (maxId != 0)
                {
                    var sightings = result?.Item2;
                    cancellationToken?.ThrowIfCancellationRequested();
                    
                    var aggregates = sightings.ToVerbatims().ToArray();
                    nrSightingsHarvested += aggregates.Length;

                    // Add sightings to MongoDb
                    await _mvmObservationVerbatimRepository.AddManyAsync(aggregates);

                    var batchDataLastModified = aggregates.Select(a => a.Modified).Max();

                    if (batchDataLastModified.HasValue && batchDataLastModified.Value > dataLastModified)
                    {
                        dataLastModified = batchDataLastModified.Value;
                    }

                    if (_mvmServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _mvmServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
                        break;
                    }

                    result = await _mvmObservationService.GetAsync(maxId + 1);
                    maxId = result?.Item1 ?? 0;
                }

                _logger.LogInformation("Finished harvesting sightings for MVM data provider");

                // Update harvest info
                harvestInfo.DataLastModified = dataLastModified == DateTime.MinValue ? (DateTime?)null : dataLastModified;
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("MVM harvest was cancelled.");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest MVM");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }
    }
}
