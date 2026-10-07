using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Moq;
using SOS.Harvest.Harvesters;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models;
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
    [Fact]
    public async Task HarvestTaxonListsAsync_LoadsActualResourceAndPreservesDefinitionsAndTranslations()
    {
        var filePath = Path.Combine(Path.GetDirectoryName(typeof(TaxonListHarvester).Assembly.Location)!,
            "Resources", "TaxonLists.json");
        using var definitions = JsonDocument.Parse(await File.ReadAllTextAsync(filePath, TestContext.Current.CancellationToken));
        var service = new Mock<ITaxonListService>();
        service.Setup(s => s.GetTaxaAsync(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync((IEnumerable<int> ids) => ids.Select(id => new NatureConservationListTaxa
            {
                ListId = id,
                TaxonInformation = [new() { Id = 103040, ScientificName = "Test species", SwedishName = "Testart" }]
            }).ToList());
        var repository = new Mock<ITaxonListRepository>();
        var session = new CollectionSession<TaxonList>("TaxonList", "TaxonList_tmp",
            Mock.Of<IMongoCollection<TaxonList>>(), Mock.Of<IMongoCollection<TaxonList>>());
        repository.Setup(r => r.CreateSession()).Returns(session);
        List<TaxonList>? storedLists = null;
        repository.Setup(r => r.AddManyAsync(It.IsAny<IEnumerable<TaxonList>>(), session.TempCollection, true))
            .Callback<IEnumerable<TaxonList>, IMongoCollection<TaxonList>, bool>((lists, _, _) => storedLists = lists.ToList())
            .ReturnsAsync(true);
        var taxa = new Mock<ITaxonRepository>();
        taxa.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Taxon>());
        var cache = new Mock<ICacheManager>();
        var harvester = new TaxonListHarvester(service.Object, repository.Object, taxa.Object, cache.Object,
            NullLogger<TaxonListHarvester>.Instance);

        var result = await harvester.HarvestTaxonListsAsync();

        Assert.Equal(RunStatus.Success, result.Status);
        Assert.NotNull(storedLists);
        Assert.Equal(definitions.RootElement.GetArrayLength(), result.Count);
        Assert.Equal(result.Count, storedLists.Count);
        foreach (var definition in definitions.RootElement.EnumerateArray())
        {
            var list = Assert.Single(storedLists, l => l.Id == definition.GetProperty("Id").GetInt32());
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
        }
        repository.Verify(r => r.PermanentizeCollectionAsync(session, storedLists.Count), Times.Once);
        cache.Verify(c => c.ClearAsync(Cache.TaxonLists), Times.Once);
    }
}
