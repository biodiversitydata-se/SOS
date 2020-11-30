using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SOS.Lib.Factories;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Taxon manager
    /// </summary>
    public class TaxonManager : ITaxonManager
    {
        private static readonly object InitLock = new object();
        private readonly ILogger<TaxonManager> _logger;
        private readonly ITaxonRepository _processedTaxonRepository;
        private readonly IMemoryCache _memoryCache;
        private const string TaxonTreeCacheKey = "TaxonTree";

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        public TaxonManager(ITaxonRepository processedTaxonRepository,
            IMemoryCache memoryCache,
            ILogger<TaxonManager> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _processedTaxonRepository = processedTaxonRepository ??
                                        throw new ArgumentNullException(nameof(processedTaxonRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// </summary>
        public TaxonTree<IBasicTaxon> TaxonTree
        {
            get
            {
                TaxonTree<IBasicTaxon> taxonTree;
                if (!_memoryCache.TryGetValue(TaxonTreeCacheKey, out taxonTree))
                {
                    lock (InitLock)
                    {
                        if (!_memoryCache.TryGetValue(TaxonTreeCacheKey, out taxonTree))
                        {
                            taxonTree = GetTaxonTreeAsync().Result;
                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                            _memoryCache.Set(TaxonTreeCacheKey, taxonTree, cacheEntryOptions);
                        }
                    }
                }

                return taxonTree;
            }
        }

        private async Task<TaxonTree<IBasicTaxon>> GetTaxonTreeAsync()
        {
            var taxa = await GetBasicTaxaAsync();
            var taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);
            return taxonTree;
        }

        private async Task<IEnumerable<BasicTaxon>> GetBasicTaxaAsync()
        {
            try
            {
                const int batchSize = 200000;
                var skip = 0;
                var tmpTaxa = await _processedTaxonRepository.GetBasicTaxonChunkAsync(skip, batchSize);
                var taxa = new List<BasicTaxon>();

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
    }
}