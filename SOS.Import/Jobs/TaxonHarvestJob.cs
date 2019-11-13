using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Interfaces;
using SOS.Import.Jobs.Interfaces;

namespace SOS.Import.Jobs
{
    /// <summary>
    /// Taxon harvest
    /// </summary>
    public class TaxonHarvestJob : ITaxonHarvestJob
    {
        private readonly ITaxonFactory _taxonFactory;
        private readonly ILogger<TaxonHarvestJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonFactory"></param>
        /// <param name="logger"></param>
        public TaxonHarvestJob(ITaxonFactory taxonFactory, ILogger<TaxonHarvestJob> logger)
        {
            _taxonFactory = taxonFactory ?? throw new ArgumentNullException(nameof(taxonFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run()
        {
            _logger.LogDebug("Start Taxon Harvest Job");

            var success = await _taxonFactory.HarvestAsync();

            _logger.LogDebug($"End taxon Harvest Job. Success: {success}");

            // return result of all harvests
            return success;
        }
    }
}
