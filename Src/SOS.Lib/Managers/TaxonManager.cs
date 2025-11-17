using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Factories;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.Managers;

/// <summary>
///     Taxon manager
/// </summary>
public class TaxonManager : ITaxonManager
{        
    private readonly ILogger<TaxonManager> _logger;
    private readonly ITaxonRepository _processedTaxonRepository;
    private readonly ITaxonListRepository _taxonListRepository;
    private readonly IClassCache<TaxonTree<IBasicTaxon>> _taxonTreeCache;
    private readonly IClassCache<TaxonListSetsById> _taxonListSetsByIdCache;
    private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private SemaphoreSlim _taxonListSemaphore = new SemaphoreSlim(1, 1);   

    private void OnTaxonTreeCacheExpiresSoon(object sender, EventArgs e)
    {
        _logger.LogInformation("OnTaxonTreeCacheExpireSoon");
        var taxonTree = FetchTaxonTreeAsync().Result;
        _taxonTreeCache.Set(taxonTree);
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
        _taxonTreeCache.CacheExpireSoon += OnTaxonTreeCacheExpiresSoon;
        _logger.LogInformation("TaxonManager created");
    }        

    public async Task<TaxonTree<IBasicTaxon>> GetTaxonTreeAsync()
    {
        var taxonTree = _taxonTreeCache.Get();
        if (taxonTree == null)
        {
            await _semaphore.WaitAsync();

            try
            {
                taxonTree = _taxonTreeCache.Get();
                if (taxonTree == null)
                {
                    taxonTree = await FetchTaxonTreeAsync();
                    _taxonTreeCache.Set(taxonTree);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return taxonTree;
    }

    public async Task<Dictionary<int, (HashSet<int> Taxa, HashSet<int> WithUnderlyingTaxa)>> GetTaxonListSetByIdAsync()
    {            
        var taxonListSetsById = _taxonListSetsByIdCache.Get();
        if (taxonListSetsById == null)
        {
            await _taxonListSemaphore.WaitAsync();

            try
            {
                taxonListSetsById = _taxonListSetsByIdCache.Get();
                if (taxonListSetsById == null)
                {

                    taxonListSetsById = await GetTaxonListSetsAsync();
                    _taxonListSetsByIdCache.Set(taxonListSetsById);
                }
            }
            finally
            {
                _taxonListSemaphore.Release();
            }
        }

        return taxonListSetsById;
    }

    private async Task<TaxonListSetsById> GetTaxonListSetsAsync()
    {
        var taxonListSetById = new TaxonListSetsById();
        var taxonLists = await _taxonListRepository.GetAllAsync();
        foreach (var taxonList in taxonLists)
        {
            var tuple = (Taxa: new HashSet<int>(), WithUnderlyingTaxa: new HashSet<int>());
            tuple.Taxa = taxonList.Taxa.Select(m => m.Id).ToHashSet();
            var taxonTree = await GetTaxonTreeAsync();
            tuple.WithUnderlyingTaxa = taxonTree.GetUnderlyingTaxonIds(tuple.Taxa, true).ToHashSet();
            taxonListSetById.TryAdd(taxonList.Id, tuple);
        }

        return taxonListSetById;
    }

    private async Task<TaxonTree<IBasicTaxon>> FetchTaxonTreeAsync()
    {
        try
        {
            _logger.LogInformation("FetchTaxonTreeAsync() - Get all taxa from MongoDB and create taxon tree");
            var taxa = await GetBasicTaxaAsync();
            var taxaDictionary = new Dictionary<int, IBasicTaxon>();
            foreach (var taxon in taxa)
            {
                // Make sure no duplicates exists
                taxaDictionary.TryAdd(taxon.Id, taxon);
            }
            return TaxonTreeFactory.CreateTaxonTree(taxaDictionary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FetchTaxonTreeAsync()");
            throw;
        }
    }

    private async Task<IEnumerable<IBasicTaxon>> GetBasicTaxaAsync()
    {
        try
        {
            var builder = Builders<Taxon>.Projection;
            ProjectionDefinition<Taxon, BasicTaxon> projection = builder.Expression(m => new BasicTaxon
            {
                Id = m.Id,
                SecondaryParentDyntaxaTaxonIds = m.SecondaryParentDyntaxaTaxonIds,
                ScientificName = m.ScientificName,
                ScientificNameAuthorship = m.ScientificNameAuthorship,
                VernacularName = m.VernacularName,
                Attributes = m.Attributes
            });

            var taxa = await _processedTaxonRepository.GetAllAsync(projection);
            return taxa;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get chunk of taxa");
            return null;
        }
    }
}