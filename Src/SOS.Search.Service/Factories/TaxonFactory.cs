using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Interfaces;
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
        private TaxonTree<IBasicTaxon> _taxonTree;
        static object _initLock = new object();

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

        private async Task<TaxonTree<IBasicTaxon>> GetTaxonTreeAsync()
        {
            var taxa = await GetBasicTaxaAsync();
            var taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);
            return taxonTree;
        }

        public TaxonTree<IBasicTaxon> TaxonTree
        {
            get
            {
                if (_taxonTree == null)
                {
                    lock (_initLock)
                    {
                        if (_taxonTree == null)
                        {
                            _taxonTree = GetTaxonTreeAsync().Result;
                        }
                    }
                }

                return _taxonTree;
            }
        }

        private async Task<IEnumerable<ProcessedBasicTaxon>> GetBasicTaxaAsync()
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

        private async Task<IEnumerable<ProcessedTaxon>> GetTaxaAsync()
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