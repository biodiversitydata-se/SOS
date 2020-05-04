using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Repositories.Destination.VirtualHerbarium.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Observations
{
    public class VirtualHerbariumObservationHarvester : Interfaces.IVirtualHerbariumObservationHarvester
    {
        private readonly IVirtualHerbariumObservationService _virtualHerbariumObservationService;
        private readonly IVirtualHerbariumObservationVerbatimRepository _virtualHerbariumObservationVerbatimRepository;
        private readonly ILogger<VirtualHerbariumObservationHarvester> _logger;
        private readonly VirtualHerbariumServiceConfiguration _virtualHerbariumServiceConfiguration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="virtualHerbariumObservationService"></param>
        /// <param name="virtualHerbariumObservationVerbatimRepository"></param>
        /// <param name="virtualHerbariumServiceConfiguration"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumObservationHarvester(
            IVirtualHerbariumObservationService virtualHerbariumObservationService,
            IVirtualHerbariumObservationVerbatimRepository virtualHerbariumObservationVerbatimRepository,
            VirtualHerbariumServiceConfiguration virtualHerbariumServiceConfiguration,
            ILogger<VirtualHerbariumObservationHarvester> logger)
        {
            _virtualHerbariumObservationService = virtualHerbariumObservationService ?? throw new ArgumentNullException(nameof(virtualHerbariumObservationService));
            _virtualHerbariumObservationVerbatimRepository = virtualHerbariumObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(virtualHerbariumObservationVerbatimRepository));
            _virtualHerbariumServiceConfiguration = virtualHerbariumServiceConfiguration ?? throw new ArgumentNullException(nameof(virtualHerbariumServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken  cancellationToken)
        {
            var harvestInfo = new HarvestInfo(nameof(VirtualHerbariumObservationVerbatim), DataSet.VirtualHerbariumObservations, DateTime.Now);
            harvestInfo.Status = RunStatus.Failed;

            try
            {
                var start = DateTime.Now;
                _logger.LogInformation("Start harvesting sightings for Virtual Herbarium data provider");

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for Virtual Herbarium verbatim collection");
                await _virtualHerbariumObservationVerbatimRepository.DeleteCollectionAsync();
                await _virtualHerbariumObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for Virtual Herbarium verbatim collection");

                var localitiesXml = await _virtualHerbariumObservationService.GetLocalitiesAsync();
                var localities = localitiesXml.ToLocalityDictionary();

                var pageIndex = 1;
                var nrSightingsHarvested = 0;
                var fromDate = new DateTime(1628, 1, 1);
                _logger.LogInformation($"Start getting observations page: { pageIndex }");
                var observations = await _virtualHerbariumObservationService.GetAsync(fromDate, pageIndex, 10000);
                _logger.LogInformation($"Finish getting observations page: { pageIndex }");

                while (observations != null)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    if (_virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested.HasValue &&
                        nrSightingsHarvested >= _virtualHerbariumServiceConfiguration.MaxNumberOfSightingsHarvested)
                    {
               //         break;
                    }

                    var verbatims = observations.ToVerbatims(localities)?.ToArray();
                    nrSightingsHarvested += verbatims?.Count() ?? 0;

                    // Add sightings to MongoDb
                    await _virtualHerbariumObservationVerbatimRepository.AddManyAsync(verbatims);

                    pageIndex++;
                    _logger.LogInformation($"Start getting observations page: { pageIndex }");
                    observations = await _virtualHerbariumObservationService.GetAsync(new DateTime(1900, 1, 1), pageIndex, 10000);
                    _logger.LogInformation($"Finish getting observations page: { pageIndex }");
                }

                _logger.LogInformation("Finished harvesting sightings for Virtual Herbarium data provider");

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = nrSightingsHarvested;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Virtual Herbarium harvest was cancelled.");
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to harvest Virtual Herbarium");
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }
    }
}
