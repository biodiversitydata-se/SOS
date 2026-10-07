using SOS.Api.Contracts.Observations.Requests;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Factories;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Managers;

/// <summary>
///     Taxon manager
/// </summary>
public class TaxonManager : ITaxonManager
{
    private readonly ILogger<TaxonManager> _logger;
    private readonly ITaxonRepository _processedTaxonRepository;
    private readonly ITaxonListRepository _taxonListRepository;
    private readonly IArtportalenTaxonRepository _processedArtportalenTaxonRepository;
    private readonly IClassCache<Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>> _taxonTreeCache;
    private readonly IClassCache<TaxonListSetsById> _taxonListSetsByIdCache;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _taxonListSemaphore = new SemaphoreSlim(1, 1);

    private void OnTaxonTreeCacheExpiresSoon(object sender, EventArgs e)
    {
        _ = RefreshTaxonTreeCacheAsync();
    }

    private async Task RefreshTaxonTreeCacheAsync()
    {
        try
        {
            _logger.LogInformation("OnTaxonTreeCacheExpiresSoon");

            await _semaphore.WaitAsync();
            try
            {
                var cache = _taxonTreeCache.Get();
                if (cache == null || cache.Count == 0)
                    return;

                var refreshedCache = new Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>(cache.Count);
                foreach (var taxonomyType in cache.Keys)
                {
                    refreshedCache[taxonomyType] = await FetchTaxonTreeAsync(taxonomyType);
                }

                _taxonTreeCache.Set(refreshedCache);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh taxon tree cache");
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="processedTaxonRepository"></param>
    /// <param name="taxonListRepository"></param>
    /// <param name="taxonTreeCache"></param>
    /// <param name="taxonListSetsByIdCache"></param>
    /// <param name="artportalenTaxonRepository"></param>
    /// <param name="logger"></param>
    public TaxonManager(ITaxonRepository processedTaxonRepository,
        ITaxonListRepository taxonListRepository,
        IClassCache<Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>> taxonTreeCache,
        IClassCache<TaxonListSetsById> taxonListSetsByIdCache,
        IArtportalenTaxonRepository artportalenTaxonRepository,
        ILogger<TaxonManager> logger)
    {

        _processedTaxonRepository = processedTaxonRepository ??
                                    throw new ArgumentNullException(nameof(processedTaxonRepository));
        _taxonListRepository = taxonListRepository ??
                               throw new ArgumentNullException(nameof(taxonListRepository));
        _taxonTreeCache = taxonTreeCache ?? throw new ArgumentNullException(nameof(taxonTreeCache));
        _taxonListSetsByIdCache = taxonListSetsByIdCache ?? throw new ArgumentNullException(nameof(taxonListSetsByIdCache));
        _processedArtportalenTaxonRepository = artportalenTaxonRepository ?? throw new ArgumentNullException(nameof(artportalenTaxonRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _taxonTreeCache.CacheExpireSoon += OnTaxonTreeCacheExpiresSoon;
        _logger.LogInformation("TaxonManager created");
    }

    public async Task<TaxonTree<IBasicTaxon>> GetTaxonTreeAsync(
        TaxonomyType taxonomyType = TaxonomyType.Dyntaxa)
    {
        var cache = _taxonTreeCache.Get();
        if (cache != null && cache.TryGetValue(taxonomyType, out var existingTree))
        {
            return existingTree;
        }

        await _semaphore.WaitAsync();
        try
        {
            cache = _taxonTreeCache.Get();
            if (cache != null && cache.TryGetValue(taxonomyType, out existingTree))
            {
                return existingTree;
            }

            var fetchedTree = await FetchTaxonTreeAsync(taxonomyType);
            var updatedCache = cache == null
                ? new Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>()
                : new Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>(cache);
            updatedCache[taxonomyType] = fetchedTree;
            _taxonTreeCache.Set(updatedCache);

            return fetchedTree;
        }
        finally
        {
            _semaphore.Release();
        }
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

    private async Task<TaxonTree<IBasicTaxon>> FetchTaxonTreeAsync(TaxonomyType taxonomyType = TaxonomyType.Dyntaxa)
    {
        try
        {
            _logger.LogInformation("FetchTaxonTreeAsync() - Get all taxa from MongoDB and create taxon tree");
            IEnumerable<IBasicTaxon> taxa;

            if (taxonomyType == TaxonomyType.Artportalen)
            {
                taxa = await GetArtportalenTaxaAsync();
            }
            else
            {
                taxa = await GetBasicTaxaAsync();
            }

            var taxaDictionary = new Dictionary<int, IBasicTaxon>();
            foreach (var taxon in taxa)
            {
                // Make sure no duplicates exists
                taxaDictionary.TryAdd(taxon.Id, taxon);
            }
            return TaxonTreeFactory.CreateTaxonTree(taxaDictionary, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FetchTaxonTreeAsync()");
            throw;
        }
    }

    private async Task<IEnumerable<IBasicTaxon>> GetArtportalenTaxaAsync()
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

            return await _processedArtportalenTaxonRepository.GetAllAsync(projection);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get chunk of taxa");
            return null;
        }
    }

    private async Task<IEnumerable<IBasicTaxon>> GetBasicTaxaAsync(bool includeAcceptedOnly = true)
    {
        try
        {
            var builder = Builders<Taxon>.Projection;
            ProjectionDefinition<Taxon, BasicTaxon> projection = builder.Expression(m => new BasicTaxon
            {
                Id = m.Id,
                SecondaryParentDyntaxaTaxonIds = m.SecondaryParentDyntaxaTaxonIds,
                IsNotAccepted = m.IsNotAccepted,
                ScientificName = m.ScientificName,
                ScientificNameAuthorship = m.ScientificNameAuthorship,
                VernacularName = m.VernacularName,
                Attributes = m.Attributes
            });

            var taxa = await _processedTaxonRepository.GetAllAsync(projection);
            if (includeAcceptedOnly)
            {
                taxa = [.. taxa.Where(t => !t.IsNotAccepted)];
            }
            return taxa;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get chunk of taxa");
            return null;
        }
    }

    public async Task<IEnumerable<TaxonList>> GetTaxonListsAsync()
    {
        return await _taxonListRepository.GetAllAsync();
    }
}
