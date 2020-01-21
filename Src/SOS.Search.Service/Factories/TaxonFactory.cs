using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.TaxonTree;
using SOS.Search.Service.Factories.Interfaces;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Factories
{
    public class TaxonFactory : ITaxonFactory
    {
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private readonly ILogger<TaxonFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="logger"></param>
        public TaxonFactory(
            IProcessedTaxonRepository processedTaxonRepository,
            ILogger<TaxonFactory> logger)
        {
            _processedTaxonRepository = processedTaxonRepository ??
                                             throw new ArgumentNullException(nameof(processedTaxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //public async Task<TaxonTree<object>> GetTaxonTreeAsync()
        //{
        //    var taxa = await GetTaxaAsync();
        //    var taxonTree = TaxonTreeFactory.CreateTaxonTree<object>(taxa);
        //    return taxonTree;
        //}

        public async Task<IEnumerable<ProcessedBasicTaxon>> GetBasicTaxaAsync()
        {
            try
            {
                const int batchSize = 200000;
                var skip = 0;
                var tmpTaxa = await _processedTaxonRepository.GetBasicTaxonChunkAsync(skip, batchSize);
                var taxa = new List<ProcessedBasicTaxon>();

                while (tmpTaxa?.Any() ?? false)
                {
                    taxa.AddRange(tmpTaxa);
                    skip += tmpTaxa.Count();
                    tmpTaxa = await _processedTaxonRepository.GetBasicTaxonChunkAsync(skip, batchSize);
                }

                return taxa;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of taxa");
                return null;
            }
        }


        /// <inheritdoc />
        public async Task<IEnumerable<ProcessedTaxon>> GetTaxaAsync()
        {
            try
            {
                const int batchSize = 200000;
                var skip = 0;
                var tmpTaxa = await _processedTaxonRepository.GetChunkAsync(skip, batchSize);
                var taxa = new List<ProcessedTaxon>();

                while (tmpTaxa?.Any() ?? false)
                {
                    taxa.AddRange(tmpTaxa);
                    skip += tmpTaxa.Count();
                    tmpTaxa = await _processedTaxonRepository.GetChunkAsync(skip, batchSize);
                }

                return taxa;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get chunk of taxa");
                return null;
            }
        }
    }
}