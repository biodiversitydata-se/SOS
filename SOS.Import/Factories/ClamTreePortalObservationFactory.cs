using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Destination.ClamTreePortal.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.ClamTreePortal;

namespace SOS.Import.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class ClamTreePortalObservationFactory : Interfaces.IClamTreePortalObservationFactory
    {
        private readonly IClamObservationVerbatimRepository _clamObservationVerbatimRepository;
        private readonly ITreeObservationVerbatimRepository _treeObservationVerbatimRepository;
        private readonly IClamTreeObservationService _clamTreeObservationService;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<ClamTreePortalObservationFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clamObservationVerbatimRepository"></param>
        /// <param name="treeObservationVerbatimRepository"></param>
        /// <param name="clamTreeObservationService"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public ClamTreePortalObservationFactory(
            IClamObservationVerbatimRepository clamObservationVerbatimRepository,
            ITreeObservationVerbatimRepository treeObservationVerbatimRepository,
            IClamTreeObservationService clamTreeObservationService,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<ClamTreePortalObservationFactory> logger)
        {
            _clamObservationVerbatimRepository = clamObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(clamObservationVerbatimRepository));
            _treeObservationVerbatimRepository = treeObservationVerbatimRepository ?? throw new ArgumentNullException(nameof(treeObservationVerbatimRepository));
            _clamTreeObservationService = clamTreeObservationService ?? throw new ArgumentNullException(nameof(clamTreeObservationService));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Aggregate clams
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HarvestClamsAsync()
        {
            try
            {
                _logger.LogDebug("Start storing clams verbatim");
                var items = await _clamTreeObservationService.GetClamObservationsAsync();

                await _clamObservationVerbatimRepository.DeleteCollectionAsync();
                await _clamObservationVerbatimRepository.AddCollectionAsync();
                await _clamObservationVerbatimRepository.AddManyAsync(items);
                
                _logger.LogDebug("Finish storing clams verbatim");
                
                // Update harvest info
                return await _harvestInfoRepository.UpdateHarvestInfoAsync(
                    nameof(ClamObservationVerbatim),
                    DataProviderId.ClamAndTreePortal,
                    items?.Count() ?? 0);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of clams");
                return false;
            }
        }

        /// <summary>
        /// Aggregate trees
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HarvestTreesAsync(IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Start storing trees verbatim");

                await _treeObservationVerbatimRepository.DeleteCollectionAsync();
                await _treeObservationVerbatimRepository.AddCollectionAsync();

                var totalCount = 0;
                var pageNumber = 1;
                const int pageSize = 500000;

                var items = await _clamTreeObservationService.GetTreeObservationsAsync(pageNumber, pageSize);

                while (items?.Any() ?? false)
                {
                    cancellationToken?.ThrowIfCancellationRequested();
                    await _treeObservationVerbatimRepository.AddManyAsync(items);

                    totalCount += items.Count();
                    pageNumber++;
                    items = await _clamTreeObservationService.GetTreeObservationsAsync(pageNumber, pageSize);
                }

                _logger.LogDebug("Finish storing tree verbatim");

                // Update harvest info
                return await _harvestInfoRepository.UpdateHarvestInfoAsync(
                    nameof(TreeObservationVerbatim),
                    DataProviderId.ClamAndTreePortal,
                    totalCount);
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Tree harvest was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of trees");
                return false;
            }
        }
    }
}
