using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Harvesters;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;
using System.Text.Json;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters;

public class TaxonListHarvesterTests
{
    private readonly Mock<ITaxonListService> _service = new();
    private readonly Mock<ITaxonListRepository> _repository = new(MockBehavior.Strict);
    private readonly Mock<ITaxonRepository> _taxa = new();
    private readonly Mock<ICacheManager> _cache = new(MockBehavior.Strict);
    private readonly Dictionary<int, TaxonList> _storedLists = [];
    private readonly TaxonListHarvester _harvester;

    public TaxonListHarvesterTests()
    {
        _repository.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<TaxonList>()))
            .Callback<int, TaxonList>((id, list) => _storedLists[id] = list)
            .ReturnsAsync(true);
        _taxa.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Taxon>
        {
            new() { Id = 103040, Attributes = new() { SensitivityCategory = new() { Id = 3 } } }
        });
        _cache.Setup(c => c.ClearAsync(Cache.TaxonLists)).ReturnsAsync(true);
        _harvester = new TaxonListHarvester(_service.Object, _repository.Object, _taxa.Object, _cache.Object,
            NullLogger<TaxonListHarvester>.Instance);
    }

    [Fact]
    public async Task HarvestTaxonListsAsync_LoadsActualResourceAndPreservesDefinitionsAndTranslations()
    {
        using var definitions = JsonDocument.Parse(await ReadDefinitionsJsonAsync());
        _service.Setup(s => s.GetTaxaAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync((IEnumerable<int> ids) => ids.Select(id => new NatureConservationListTaxa
            {
                ListId = id,
                TaxonInformation = [new() { Id = 103040, ScientificName = "Test species", SwedishName = "Testart" }]
            }).ToList());

        var result = await _harvester.HarvestTaxonListsAsync();

        Assert.Equal(RunStatus.Success, result.Status);
        Assert.Equal(definitions.RootElement.GetArrayLength(), result.Count);
        Assert.Equal(result.Count, _storedLists.Count);
        Assert.InRange(result.End, result.Start, DateTime.Now);
        foreach (var definition in definitions.RootElement.EnumerateArray())
        {
            var list = _storedLists[definition.GetProperty("Id").GetInt32()];
            Assert.Equal(definition.GetProperty("TaxonListServiceId").GetInt32(), list.TaxonListServiceId);
            Assert.Equal(definition.TryGetProperty("ParentId", out var parentId) ? parentId.GetInt32() : (int?)null, list.ParentId);
            Assert.Equal(definition.TryGetProperty("AllowNotaxa", out var allowNoTaxa) && allowNoTaxa.GetBoolean(), list.AllowNotaxa);
            var names = definition.GetProperty("Names").EnumerateArray().ToArray();
            Assert.Equal(names.Length, list.Names.Count);
            foreach (var name in names)
            {
                var translation = Assert.Single(list.Names, n => n.CultureCode == name.GetProperty("CultureCode").GetString());
                Assert.Equal(name.GetProperty("Value").GetString(), translation.Value);
            }
            var taxon = Assert.Single(list.Taxa);
            Assert.Equal(103040, taxon.Id);
            Assert.Equal("Test species", taxon.ScientificName);
            Assert.Equal("Testart", taxon.SwedishName);
            Assert.Equal(3, taxon.SensitivityCategory);
            _repository.Verify(r => r.UpdateAsync(list.Id, list), Times.Once);
        }
        _repository.VerifyNoOtherCalls();
        _cache.Verify(c => c.ClearAsync(Cache.TaxonLists), Times.Once);
    }

    [Theory]
    [InlineData("empty", false)]
    [InlineData("null", false)]
    [InlineData("missing", false)]
    [InlineData("empty", true)]
    [InlineData("null", true)]
    [InlineData("missing", true)]
    public async Task HarvestTaxonListsAsync_PartialResponsePreservesSkippedAndUnrelatedDocuments(
        string responseKind, bool allowNoTaxa)
    {
        var definitions = await LoadDefinitionsAsync();
        var skipped = definitions.First(list => list.AllowNotaxa == allowNoTaxa);
        var existing = CreateExistingList(skipped);
        var snapshot = JsonSerializer.Serialize(existing);
        _storedLists[existing.Id] = existing;
        var unrelated = new TaxonList { Id = -1, Names = [], Taxa = [] };
        _storedLists[unrelated.Id] = unrelated;
        var response = definitions.Where(list => list.Id != skipped.Id).Select(CreateResponse).ToList();
        if (responseKind != "missing")
        {
            response.Add(new NatureConservationListTaxa
            {
                ListId = skipped.TaxonListServiceId,
                TaxonInformation = responseKind == "empty" ? [] : null
            });
        }
        _service.Setup(s => s.GetTaxaAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(response);

        var result = await _harvester.HarvestTaxonListsAsync();

        Assert.Equal(RunStatus.Success, result.Status);
        Assert.Equal(definitions.Count - 1, result.Count);
        Assert.Equal(definitions.Count + 1, _storedLists.Count);
        Assert.Equal(snapshot, JsonSerializer.Serialize(_storedLists[skipped.Id]));
        Assert.Same(unrelated, _storedLists[unrelated.Id]);
        foreach (var definition in definitions.Where(list => list.Id != skipped.Id))
        {
            Assert.Equal(103040, Assert.Single(_storedLists[definition.Id].Taxa).Id);
        }
        Assert.InRange(result.End, result.Start, DateTime.Now);
        _repository.Verify(r => r.UpdateAsync(skipped.Id, It.IsAny<TaxonList>()), Times.Never);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<TaxonList>()), Times.Exactly(result.Count));
        _repository.VerifyNoOtherCalls();
        _cache.Verify(c => c.ClearAsync(Cache.TaxonLists), Times.Once);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("empty")]
    [InlineData("emptyTaxa")]
    [InlineData("nullTaxa")]
    public async Task HarvestTaxonListsAsync_WithoutRetrievedTaxaFailsWithoutChangingData(string responseKind)
    {
        var definitions = await LoadDefinitionsAsync();
        var existing = CreateExistingList(definitions[0]);
        _storedLists[existing.Id] = existing;
        List<NatureConservationListTaxa>? response = responseKind switch
        {
            "null" => null,
            "empty" => [],
            _ => definitions.Select(list => new NatureConservationListTaxa
            {
                ListId = list.TaxonListServiceId,
                TaxonInformation = responseKind == "emptyTaxa" ? [] : null
            }).ToList()
        };
        _service.Setup(s => s.GetTaxaAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(response);

        var result = await _harvester.HarvestTaxonListsAsync();

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.Equal(0, result.Count);
        Assert.Same(existing, Assert.Single(_storedLists.Values));
        Assert.InRange(result.End, result.Start, DateTime.Now);
        _repository.VerifyNoOtherCalls();
        _cache.VerifyNoOtherCalls();
        _taxa.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task HarvestTaxonListsAsync_CreatesMissingListsAndAcceptsUnchangedDocuments()
    {
        var definition = (await LoadDefinitionsAsync())[0];
        _service.Setup(s => s.GetTaxaAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([CreateResponse(definition)]);
        // The repository returns false for both successful inserts and unchanged replacements.
        _repository.Setup(r => r.UpdateAsync(definition.Id, It.IsAny<TaxonList>()))
            .Callback<int, TaxonList>((id, list) => _storedLists[id] = list)
            .ReturnsAsync(false);

        var first = await _harvester.HarvestTaxonListsAsync();
        var snapshot = JsonSerializer.Serialize(Assert.Single(_storedLists.Values));
        var second = await _harvester.HarvestTaxonListsAsync();

        Assert.Equal(RunStatus.Success, first.Status);
        Assert.Equal(RunStatus.Success, second.Status);
        Assert.Equal(1, first.Count);
        Assert.Equal(1, second.Count);
        Assert.Equal(snapshot, JsonSerializer.Serialize(Assert.Single(_storedLists.Values)));
        _repository.Verify(r => r.UpdateAsync(definition.Id, It.IsAny<TaxonList>()), Times.Exactly(2));
        _repository.VerifyNoOtherCalls();
        _cache.Verify(c => c.ClearAsync(Cache.TaxonLists), Times.Exactly(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task HarvestTaxonListsAsync_WriteFailurePreservesDocumentsAndClearsCache(int failingIndex)
    {
        var definitions = (await LoadDefinitionsAsync()).Take(2).ToList();
        foreach (var definition in definitions)
        {
            _storedLists[definition.Id] = CreateExistingList(definition);
        }
        var failedDefinition = definitions[failingIndex];
        var existing = _storedLists[failedDefinition.Id];
        _service.Setup(s => s.GetTaxaAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(definitions.Select(CreateResponse).ToList());
        _repository.Setup(r => r.UpdateAsync(failedDefinition.Id, It.IsAny<TaxonList>()))
            .ThrowsAsync(new InvalidOperationException("Write failed"));

        var result = await _harvester.HarvestTaxonListsAsync();

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.Equal(failingIndex, result.Count);
        Assert.Equal(2, _storedLists.Count);
        Assert.Same(existing, _storedLists[failedDefinition.Id]);
        if (failingIndex > 0)
        {
            Assert.Equal(103040, Assert.Single(_storedLists[definitions[0].Id].Taxa).Id);
        }
        Assert.InRange(result.End, result.Start, DateTime.Now);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<TaxonList>()), Times.Exactly(failingIndex + 1));
        _repository.VerifyNoOtherCalls();
        _cache.Verify(c => c.ClearAsync(Cache.TaxonLists), Times.Once);
    }

    [Fact]
    public async Task HarvestTaxonListsAsync_ServiceFailureSetsEndWithoutWritingOrClearingCache()
    {
        _service.Setup(s => s.GetTaxaAsync(It.IsAny<IEnumerable<int>>()))
            .ThrowsAsync(new InvalidOperationException("Service failed"));

        var result = await _harvester.HarvestTaxonListsAsync();

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.Equal(0, result.Count);
        Assert.InRange(result.End, result.Start, DateTime.Now);
        _repository.VerifyNoOtherCalls();
        _cache.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HarvestTaxonListsAsync_CacheFailureReportsFailureAndPreservesSavedDocument()
    {
        var definition = (await LoadDefinitionsAsync())[0];
        _service.Setup(s => s.GetTaxaAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync([CreateResponse(definition)]);
        _cache.Setup(c => c.ClearAsync(Cache.TaxonLists))
            .ThrowsAsync(new InvalidOperationException("Cache unavailable"));

        var result = await _harvester.HarvestTaxonListsAsync();

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.Equal(1, result.Count);
        Assert.Equal(103040, Assert.Single(Assert.Single(_storedLists.Values).Taxa).Id);
        Assert.InRange(result.End, result.Start, DateTime.Now);
        _cache.Verify(c => c.ClearAsync(Cache.TaxonLists), Times.Once);
    }

    private static NatureConservationListTaxa CreateResponse(TaxonList definition) => new()
    {
        ListId = definition.TaxonListServiceId,
        TaxonInformation = [new() { Id = 103040, ScientificName = "Test species", SwedishName = "Testart" }]
    };

    private static TaxonList CreateExistingList(TaxonList definition) => new()
    {
        Id = definition.Id,
        TaxonListServiceId = definition.TaxonListServiceId,
        ParentId = -1,
        AllowNotaxa = definition.AllowNotaxa,
        Names = [new() { CultureCode = "sv-SE", Value = "Existing name" }],
        Taxa = [new() { Id = 1, ScientificName = "Existing species", SwedishName = "Befintlig art", SensitivityCategory = 4 }]
    };

    private static async Task<List<TaxonList>> LoadDefinitionsAsync() =>
        JsonSerializer.Deserialize<List<TaxonList>>(await ReadDefinitionsJsonAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    private static Task<string> ReadDefinitionsJsonAsync()
    {
        var filePath = Path.Combine(Path.GetDirectoryName(typeof(TaxonListHarvester).Assembly.Location)!,
            "Resources", "TaxonLists.json");
        return File.ReadAllTextAsync(filePath, TestContext.Current.CancellationToken);
    }
}
