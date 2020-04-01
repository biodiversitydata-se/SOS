using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Taxon harvest
    /// </summary>
    public class TaxonHarvestJob : ITaxonHarvestJob
    {
        private readonly ITaxonHarvester _taxonHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<TaxonHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public TaxonHarvestJob(ITaxonHarvester taxonHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<TaxonHarvestJob> logger)
        {
            _taxonHarvester = taxonHarvester ?? throw new ArgumentNullException(nameof(taxonHarvester));
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync()
        {
            _logger.LogInformation("Start Taxon Harvest Job");

            var result = await _taxonHarvester.HarvestAsync();

            _logger.LogInformation($"End taxon Harvest Job. Status: {result.Status}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            // return result of all harvests
            return result.Status.Equals(RunStatus.Success) ? true : throw new Exception("Taxon Harvest Job failed");
        }
    }
}
