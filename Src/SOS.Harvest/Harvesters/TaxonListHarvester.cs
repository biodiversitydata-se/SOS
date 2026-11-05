using Microsoft.Extensions.Logging;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;
using System.Reflection;
using System.Text.Json;

namespace SOS.Harvest.Harvesters;

/// <summary>
///     Class for harvest taxon lists.
/// </summary>
public class TaxonListHarvester : ITaxonListHarvester
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ITaxonListService _taxonListService;
    private readonly ITaxonListRepository _taxonListRepository;
    private readonly ITaxonRepository _taxonRepository;
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<TaxonListHarvester> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="taxonListService"></param>
    /// <param name="taxonListRepository"></param>
    /// <param name="taxonRepository"></param>
    /// <param name="cacheManager"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public TaxonListHarvester(
        ITaxonListService taxonListService,
        ITaxonListRepository taxonListRepository,
        ITaxonRepository taxonRepository,
        ICacheManager cacheManager,
        ILogger<TaxonListHarvester> logger)
    {
        _taxonListService = taxonListService ?? throw new ArgumentNullException(nameof(taxonListService));
        _taxonListRepository = taxonListRepository ?? throw new ArgumentNullException(nameof(taxonListRepository));
        _taxonRepository = taxonRepository ?? throw new ArgumentNullException(nameof(taxonRepository));
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HarvestInfo> HarvestTaxonListsAsync()
    {
        var harvestInfo = new HarvestInfo(nameof(TaxonList), DateTime.Now)
        {
            Status = RunStatus.Failed
        };
        var writeAttempted = false;
        try
        {
            _logger.LogDebug("Start getting taxon lists");

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath!, @"Resources/TaxonLists.json");
            var taxonLists = JsonSerializer.Deserialize<List<TaxonList>>(
                await File.ReadAllTextAsync(filePath), JsonOptions);

            if (!taxonLists?.Any() ?? true)
            {
                throw new Exception("Failed to load taxon list json");
            }

            var conservationLists =
                await _taxonListService.GetTaxaAsync(taxonLists!.Select(m => m.TaxonListServiceId));
            var conservationListById = (conservationLists ?? []).ToDictionary(list => list.ListId);
            var taxonListsToUpdate = new List<TaxonList>();
            foreach (var taxonList in taxonLists)
            {
                if (!conservationListById.TryGetValue(taxonList.TaxonListServiceId, out var conservationList)
                    || !(conservationList.TaxonInformation?.Any() ?? false))
                {
                    _logger.LogWarning(
                        "Skipping taxon list without retrieved taxa. Id: {Id}, TaxonListServiceId: {TaxonListServiceId}, Title: {Title}",
                        taxonList.Id, taxonList.TaxonListServiceId, taxonList.Names?.FirstOrDefault()?.Value);
                    continue;
                }

                taxonListsToUpdate.Add(taxonList);
            }

            if (taxonListsToUpdate.Count == 0)
            {
                _logger.LogError("No taxon lists with taxa were retrieved");
                return harvestInfo;
            }

            var taxa = (await _taxonRepository.GetAllAsync())?.Where(t => !t.IsNotAccepted).ToDictionary(t => t.Id, t => t) ?? new Dictionary<int, Taxon>();
            foreach (var taxonList in taxonListsToUpdate)
            {
                taxonList.Taxa = conservationListById[taxonList.TaxonListServiceId].TaxonInformation
                    .Select(ti => ConvertToTaxonListTaxonInformation(ti, taxa)).ToList();

                writeAttempted = true;
                // UpdateAsync also returns false for unchanged documents and successful inserts.
                await _taxonListRepository.UpdateAsync(taxonList.Id, taxonList);
                harvestInfo.Count++;
            }

            harvestInfo.Status = RunStatus.Success;

            _logger.LogInformation("Saving {Count} taxon lists succeeded", harvestInfo.Count);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed harvest of taxon lists");
            harvestInfo.Status = RunStatus.Failed;
        }
        finally
        {
            if (writeAttempted)
            {
                try
                {
                    // A failed write may still have changed data, so invalidate the cache on failure too.
                    await _cacheManager.ClearAsync(Cache.TaxonLists);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to clear taxon lists cache");
                    harvestInfo.Status = RunStatus.Failed;
                }
            }

            harvestInfo.End = DateTime.Now;
        }

        return harvestInfo;
    }

    private TaxonListTaxonInformation ConvertToTaxonListTaxonInformation(TaxonInformation taxonInformation, IDictionary<int, Taxon> taxa)
    {
        taxa.TryGetValue(taxonInformation.Id, out var taxon);
        return new TaxonListTaxonInformation()
        {
            Id = taxonInformation.Id,
            ScientificName = taxonInformation.ScientificName,
            SwedishName = taxonInformation.SwedishName,
            SensitivityCategory = taxon?.Attributes?.SensitivityCategory?.Id
        };
    }
}
