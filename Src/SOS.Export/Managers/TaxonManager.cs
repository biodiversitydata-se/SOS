using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Factories;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Export.Managers
{
    /// <summary>
    ///     Taxon manager
    /// </summary>
    public class TaxonManager : ITaxonManager
    {
        private static readonly object InitLock = new object();
        private readonly ILogger<TaxonManager> _logger;
        private readonly IProcessedTaxonRepository _processedTaxonRepository;
        private TaxonTree<IBasicTaxon> _taxonTree;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="logger"></param>
        public TaxonManager(
            IProcessedTaxonRepository processedTaxonRepository,
            ILogger<TaxonManager> logger)
        {
            _processedTaxonRepository = processedTaxonRepository ??
                                        throw new ArgumentNullException(nameof(processedTaxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public TaxonTree<IBasicTaxon> TaxonTree
        {
            get
            {
                if (_taxonTree == null)
                {
                    lock (InitLock)
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

        private async Task<TaxonTree<IBasicTaxon>> GetTaxonTreeAsync()
        {
            var taxa = await GetBasicTaxaAsync();
            var taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);
            return taxonTree;
        }
    }
}