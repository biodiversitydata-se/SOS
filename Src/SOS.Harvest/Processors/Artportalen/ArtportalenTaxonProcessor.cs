using Microsoft.Extensions.Logging;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Factories;
using SOS.Lib.Models.ArtportalenApiService;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Harvest.Processors.Artportalen;

public class ArtportalenTaxonProcessor : IArtportalenTaxonProcessor
{
    private readonly IArtportalenApiService _artportalenApiService;
    private readonly ITaxonRepository _taxonRepository;
    private readonly IArtportalenTaxonRepository _artportalenTaxonRepository;
    private readonly ILogger<ArtportalenTaxonProcessor> _logger;

    public ArtportalenTaxonProcessor(
        IArtportalenApiService artportalenApiService,
        ITaxonRepository taxonRepository,
        IArtportalenTaxonRepository artportalenTaxonRepository,
        ILogger<ArtportalenTaxonProcessor> logger)
    {
        _artportalenApiService = artportalenApiService ?? throw new ArgumentNullException(nameof(artportalenApiService));
        _taxonRepository = taxonRepository ?? throw new ArgumentNullException(nameof(taxonRepository));
        _artportalenTaxonRepository = artportalenTaxonRepository ?? throw new ArgumentNullException(nameof(artportalenTaxonRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> ProcessArtportalenTaxaAsync()
    {
        try
        {
            var allTaxa = await _taxonRepository.GetAllAsync();
            if (allTaxa == null || !allTaxa.Any())
            {
                _logger.LogWarning("No taxa found in processed Taxon collection");
                return -1;
            }

            var artportalenTaxonRelations = await GetArtportalenTaxonRelations();
            if (artportalenTaxonRelations == null || !artportalenTaxonRelations.Any()) return -1;

            var nearestRelations = artportalenTaxonRelations.Where(m => m.Relation == 2)
                .Select(m => new TaxonRelation<int>
                {
                    IsMainRelation = false,
                    ParentTaxonId = m.ParentId,
                    ChildTaxonId = m.ChildId
                }).ToList();

            var taxonRelationsByChildId = nearestRelations
                .GroupBy(m => m.ChildTaxonId)
                .ToDictionary(g => g.Key, g => g.Select(m => m));

            var taxonTree = TaxonTreeFactory.CreateTaxonTree(
                allTaxa.ToDictionary(t => t.Id, t => t),
                _logger);

            SetMainRelations(taxonRelationsByChildId, taxonTree); //Set a main parent when there are multiple parents

            var artportalenTaxa = new List<SOS.Lib.Models.Processed.Observation.Taxon>();
            foreach (var taxon in allTaxa)
            {                
                if (taxonRelationsByChildId.TryGetValue(taxon.Attributes.DyntaxaTaxonId, out var relations))
                {
                    taxon.Attributes.ParentDyntaxaTaxonId = relations.First(r => r.IsMainRelation).ParentTaxonId;
                    taxon.SecondaryParentDyntaxaTaxonIds = relations.Where(r => !r.IsMainRelation)?.Select(r => r.ParentTaxonId);
                    artportalenTaxa.Add(taxon);
                }
            }

            artportalenTaxa.Add(allTaxa.FirstOrDefault(t => t.Attributes.DyntaxaTaxonId == 0));

            var session = _artportalenTaxonRepository.CreateSession();
            if (!await _artportalenTaxonRepository.DeleteCollectionAsync(session.TempCollection))
            {
                _logger.LogError("Failed to delete existing temp collection for ArtportalenTaxon");
                return -1;
            }

            var success = await _artportalenTaxonRepository.AddManyAsync(artportalenTaxa, session.TempCollection, useMajorityCollection: true);
            await _artportalenTaxonRepository.PermanentizeCollectionAsync(session, artportalenTaxa.Count);
            await _artportalenTaxonRepository.WaitForDataInsert(artportalenTaxa.Count, TimeSpan.FromMinutes(5));

            _logger.LogInformation($"Finished writing {artportalenTaxa.Count} artportalen taxa");

            return success ? artportalenTaxa.Count : -1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Artportalen taxa");
            return -1;
        }
    }

    private async Task<List<TaxonHierarchyItem>> GetArtportalenTaxonRelations()
    {
        _logger.LogInformation("Start fetching Artportalen taxon hierarchy and populating ArtportalenTaxon collection");
        var artportalenTaxonRelations = new List<TaxonHierarchyItem>();

        const int limit = 100000;
        int offset = 0;

        while (true)
        {
            var response = await _artportalenApiService.GetTaxonHierarchyAsync(offset, limit);
            if (response?.Data == null)
            {
                _logger.LogError($"Failed to retrieve taxon hierarchy from Artportalen API. Offset={offset}, Limit={limit}");
                return null;
            }

            artportalenTaxonRelations.AddRange(response.Data);
            offset += response.Pager.Limit;

            if (offset >= response.Pager.TotalCount)
                break;
        }

        return artportalenTaxonRelations;
    }

    private static void SetMainRelations(Dictionary<int, IEnumerable<TaxonRelation<int>>> taxonRelationsByChildId, Lib.Models.TaxonTree.TaxonTree<Lib.Models.Interfaces.IBasicTaxon> taxonTree)
    {
        foreach (var childIdGrouping in taxonRelationsByChildId)
        {
            var relationsList = childIdGrouping.Value.ToList();

            int firstGenusIndex = -1;
            for (int i = 0; i < relationsList.Count; i++)
            {
                var parentNode = taxonTree.GetTreeNode(relationsList[i].ParentTaxonId);
                if (parentNode != null && parentNode.Data.Attributes.TaxonCategory.Id == (int)TaxonCategoryId.Genus)
                {
                    firstGenusIndex = i;
                    break;
                }
            }

            if (firstGenusIndex >= 0)
            {
                relationsList[firstGenusIndex].IsMainRelation = true;
            }
            else
            {
                relationsList[0].IsMainRelation = true;
            }
        }
    }
}
