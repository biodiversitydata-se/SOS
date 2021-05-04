using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    ///     Taxon lists harvest job.
    /// </summary>
    public class TaxonListsHarvestJob : ITaxonListsHarvestJob
    {
        private readonly ITaxonListHarvester _taxonListHarvester;
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly ILogger<TaxonListsHarvestJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="taxonListHarvester"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="logger"></param>
        public TaxonListsHarvestJob(
            ITaxonListHarvester taxonListHarvester,
            IHarvestInfoRepository harvestInfoRepository,
            ILogger<TaxonListsHarvestJob> logger)
        {
            _taxonListHarvester = taxonListHarvester ?? throw new ArgumentNullException(nameof(taxonListHarvester));
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <inheritdoc />
        [DisplayName("Harvest taxon lists from TaxonListService")]
        public async Task<bool> RunHarvestTaxonListsAsync()
        {
            _logger.LogInformation("Start harvest taxon lists job");

            var result = await _taxonListHarvester.HarvestTaxonListsAsync();

            _logger.LogInformation($"End harvest taxon lists job. Result: {result.Status == RunStatus.Success}");

            // Save harvest info
            await _harvestInfoRepository.AddOrUpdateAsync(result);

            return result.Status == RunStatus.Success && result.Count > 0
                ? true
                : throw new Exception("Harvest taxon lists job failed");
        }
    }
}