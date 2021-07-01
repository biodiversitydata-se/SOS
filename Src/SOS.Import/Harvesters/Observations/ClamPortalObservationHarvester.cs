using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Harvest;
using SOS.Import.Harvesters.Observations.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Harvesters.Observations
{
    /// <summary>
    ///     Clam Portal observation harvester
    /// </summary>
    public class ClamPortalObservationHarvester : IClamPortalObservationHarvester
    {
        private readonly IClamObservationService _clamObservationService;
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly ILogger<ClamPortalObservationHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="clamObservationService"></param>
        /// <param name="logger"></param>
        public ClamPortalObservationHarvester(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IClamObservationService clamObservationService,
            ILogger<ClamPortalObservationHarvester> logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ??
                                                 throw new ArgumentNullException(
                                                     nameof(clamObservationVerbatimRepository));
            _clamObservationService =
                clamObservationService ?? throw new ArgumentNullException(nameof(clamObservationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            clamObservationVerbatimRepository.TempMode = true;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(IJobCancellationToken cancellationToken)
        {
            var harvestInfo = new HarvestInfo(DateTime.Now);
            try
            {
                _logger.LogInformation("Start harvesting sightings for clams data provider");

                // Make sure we have an empty collection.
                _logger.LogInformation("Start empty collection for clams verbatim collection");
                await _clamObservationVerbatimRepository.DeleteCollectionAsync();
                await _clamObservationVerbatimRepository.AddCollectionAsync();
                _logger.LogInformation("Finish empty collection for clams verbatim collection");

                var verbatims = await _clamObservationService.GetClamObservationsAsync();

                if (verbatims?.Any() ?? false)
                {
                    var verbatimFactory = new ClamPortalHarvestFactory();
                    verbatims = await verbatimFactory.CastEntitiesToVerbatimsAsync(verbatims);

                    await _clamObservationVerbatimRepository.AddManyAsync(verbatims);
                }

                cancellationToken?.ThrowIfCancellationRequested();

                _logger.LogInformation("Finished harvesting sightings for clams data provider");

                // Update harvest info
                harvestInfo.DataLastModified = verbatims?.Select(o => o.Modified).Max();
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = verbatims?.Count() ?? 0;

                _logger.LogInformation("Start permanentize temp collection for clams verbatim");
                await _clamObservationVerbatimRepository.PermanentizeCollectionAsync();
                _logger.LogInformation("Finish permanentize temp collection for clams verbatim");
            }
            catch (JobAbortedException e)
            {
                _logger.LogError(e, "Canceled harvest of clams");
                harvestInfo.Status = RunStatus.Canceled;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of clams");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(JobRunModes mode,
            IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }

        /// inheritdoc />
        public async Task<HarvestInfo> HarvestObservationsAsync(DataProvider provider, IJobCancellationToken cancellationToken)
        {
            throw new NotImplementedException("Not implemented for this provider");
        }
    }
}