using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Destination.ClamPortal.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamPortal;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class ClamPortalObservationFactory : Interfaces.IClamPortalObservationFactory
    {
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly IClamObservationService _clamObservationService;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<ClamPortalObservationFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="clamObservationService"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ClamPortalObservationFactory(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            IClamObservationService clamObservationService,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ClamPortalObservationFactory> logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _clamObservationService = clamObservationService ?? throw new ArgumentNullException(nameof(clamObservationService));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Aggregate clams
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HarvestClamsAsync(IJobCancellationToken cancellationToken)
        {
            try
            {
                var start = DateTime.Now;
                _logger.LogDebug("Start storing clams verbatim");
                var items = await _clamObservationService.GetClamObservationsAsync();

                await _clamObservationVerbatimRepository.DeleteCollectionAsync();
                await _clamObservationVerbatimRepository.AddCollectionAsync();
                await _clamObservationVerbatimRepository.AddManyAsync(items);
                
                _logger.LogDebug("Finish storing clams verbatim"); 
                
                cancellationToken?.ThrowIfCancellationRequested();

                // Update harvest info
                return await _harvestInfoRepository.UpdateHarvestInfoAsync(
                    nameof(ClamObservationVerbatim),
                    DataProvider.ClamPortal,
                    start,
                    DateTime.Now, 
                    items?.Count() ?? 0);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of clams");
                return false;
            }
        }
    }
}
