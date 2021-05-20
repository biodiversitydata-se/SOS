using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Factories;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.TaxonListService;

namespace SOS.Lib.Managers
{
    /// <summary>
    ///     Taxon manager
    /// </summary>
    public class TaxonManager : ITaxonManager
    {
        private static readonly object InitLock = new object();
        private readonly ILogger<TaxonManager> _logger;
        private readonly ITaxonRepository _processedTaxonRepository;
        private readonly ITaxonListRepository _taxonListRepository;
        private readonly IClassCache<TaxonTree<IBasicTaxon>> _taxonTreeCache;
        private readonly IClassCache<TaxonListSetsById> _taxonListSetsByIdCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedTaxonRepository"></param>
        /// <param name="taxonListRepository"></param>
        /// <param name="taxonTreeCache"></param>
        /// <param name="taxonListSetsByIdCache"></param>
        /// <param name="logger"></param>
        public TaxonManager(ITaxonRepository processedTaxonRepository,
            ITaxonListRepository taxonListRepository,
            IClassCache<TaxonTree<IBasicTaxon>> taxonTreeCache,
            IClassCache<TaxonListSetsById> taxonListSetsByIdCache,
            ILogger<TaxonManager> logger)
        {
            
            _processedTaxonRepository = processedTaxonRepository ??
                                        throw new ArgumentNullException(nameof(processedTaxonRepository));
            _taxonListRepository = taxonListRepository ??
                                   throw new ArgumentNullException(nameof(taxonListRepository));
            _taxonTreeCache = taxonTreeCache ?? throw new ArgumentNullException(nameof(taxonTreeCache));
            _taxonListSetsByIdCache = taxonListSetsByIdCache ?? throw new ArgumentNullException(nameof(taxonListSetsByIdCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// </summary>
        public TaxonTree<IBasicTaxon> TaxonTree
        {
            get
            {
                var taxonTree = _taxonTreeCache.Get();
                if (taxonTree == null)
                {
                    lock (InitLock)
                    {
                        taxonTree = GetTaxonTreeAsync().Result;
                        _taxonTreeCache.Set(taxonTree);
                    }
                }

                return taxonTree;
            }
        }

        public Dictionary<int, HashSet<int>> TaxonListSetById
        {
            get
            {
                var taxonListSetsById = _taxonListSetsByIdCache.Get();
                if (taxonListSetsById == null)
                {
                    lock (InitLock)
                    {
                        taxonListSetsById = GetTaxonListSetsAsync().Result;
                        _taxonListSetsByIdCache.Set(taxonListSetsById);
                    }
                }

                return taxonListSetsById;
            }
        }

        private async Task<TaxonListSetsById> GetTaxonListSetsAsync()
        {
            var taxonListSetById = new TaxonListSetsById();
            var taxonLists = await _taxonListRepository.GetAllAsync();
            foreach (var taxonList in taxonLists)
            {
                taxonListSetById.Add(taxonList.Id, new HashSet<int>());
                foreach (var taxon in taxonList.Taxa)
                {
                    taxonListSetById[taxonList.Id].Add(taxon.Id);
                }
            }

            return taxonListSetById;
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