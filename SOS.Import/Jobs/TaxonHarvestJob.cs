using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Taxon harvest
    /// </summary>
    public class TaxonHarvestJob : ITaxonHarvestJob
    {
        private readonly ITaxonFactory _taxonFactory;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<TaxonHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonFactory"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public TaxonHarvestJob(ITaxonFactory taxonFactory,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<TaxonHarvestJob> logger)
        {
            _taxonFactory = taxonFactory ?? throw new ArgumentNullException(nameof(taxonFactory));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogDebug("Start Taxon Harvest Job");

            var result = await _taxonFactory.HarvestAsync();

            _logger.LogDebug($"End taxon Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            // return result of all harvests
            return result.Status.Equals(HarvestStatus.Succeded);
        }
    }
}
