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
        private static readonly object InitTaxonTreeLock = new object();
        private static readonly object InitTaxonListLock = new object();
        private readonly ILogger<TaxonManager> _logger;
        private readonly ITaxonRepository _processedTaxonRepository;
        private readonly ITaxonListRepository _taxonListRepository;
        private readonly IClassCache<TaxonTree<IBasicTaxon>> _taxonTreeCache;
        private readonly IClassCache<TaxonListSetsById> _taxonListSetsByIdCache;

        private void OnTaxonTreeCacheReleased(object sender, EventArgs e)
        {
            PopulateTaxonTreeCache();
        }

        /// <summary>
        /// Populate taxon tree cache
        /// </summary>
        /// <returns></returns>
        private TaxonTree<IBasicTaxon> PopulateTaxonTreeCache()
        {
            lock (InitTaxonTreeLock)
            {
                var taxonTree = GetTaxonTreeAsync().Result;
                _taxonTreeCache.Set(taxonTree);
                return taxonTree;
            }
        }

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

            _taxonTreeCache.CacheReleased += OnTaxonTreeCacheReleased;
        }

        /// <summary>
        /// </summary>
        public TaxonTree<IBasicTaxon> TaxonTree
        {
            get
            {
                return _taxonTreeCache.Get() ?? PopulateTaxonTreeCache();
            }
        }

        public Dictionary<int, (HashSet<int> Taxa, HashSet<int> WithUnderlyingTaxa)> TaxonListSetById
        {
            get
            {
                var taxonListSetsById = _taxonListSetsByIdCache.Get();
                if (taxonListSetsById == null)
                {
                    lock (InitTaxonListLock)
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
                var tuple = (Taxa: new HashSet<int>(), WithUnderlyingTaxa: new HashSet<int>());
                tuple.Taxa = taxonList.Taxa.Select(m => m.Id).ToHashSet();
                tuple.WithUnderlyingTaxa = TaxonTree.GetUnderlyingTaxonIds(tuple.Taxa, true).ToHashSet();
                taxonListSetById.Add(taxonList.Id, tuple);
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
                var taxaCount = await _processedTaxonRepository.CountAllDocumentsAsync();
                const int batchSize = 10000;
                var skip = 0;
                var tasks = new List<Task<IEnumerable<BasicTaxon>>>();
               
                while(skip < taxaCount)
                {
                    tasks.Add(_processedTaxonRepository.GetBasicTaxonChunkAsync(skip, batchSize));
                    skip += batchSize;
                }
           
                await Task.WhenAll(tasks);
                var taxa = new List<BasicTaxon>();
                foreach (var task in tasks)
                {
                    taxa.AddRange(task.Result);
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