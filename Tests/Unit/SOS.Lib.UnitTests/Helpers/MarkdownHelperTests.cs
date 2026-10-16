
using SOS.Api.Contracts.Observations.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Xml.Linq;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers;

public class MarkdownHelperTests
{
    [Theory]
    [InlineData("", true, "SHARK ")]
    [InlineData(null, true, "SHARK ")]
    [InlineData("A", true, "SHARK A")]
    [InlineData("Anna", true, "SHARK A")]
    [InlineData(null, false, " ")]
    public async Task GetCitationMarkdownAsync_HandlesMissingContactNames(
        string? firstName, bool hasContactPerson, string expectedAuthor)
    {
        var dataProviders = new[]
        {
            new DataProvider
            {
                Id = 8,
                Identifier = "SHARK",
                ContactPerson = hasContactPerson ? new ContactPerson { FirstName = firstName, LastName = "SHARK" } : null,
                Names = new[] { new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "SHARK dataset" } },
                Organizations = new[] { new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "SMHI" } }
            },
            new DataProvider
            {
                Id = 1,
                Identifier = "Artportalen",
                ContactPerson = new ContactPerson { FirstName = "Johan", LastName = "Liljeblad" },
                Names = new[] { new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "Artportalen" } },
                Organizations = new[] { new VocabularyValueTranslation { CultureCode = "sv-SE", Value = "SLU Artdatabanken" } }
            }
        };
        var dataProviderCache = new Mock<IDataProviderCache>(MockBehavior.Strict);
        dataProviderCache.Setup(cache => cache.GetAllAsync()).ReturnsAsync(dataProviders);
        dataProviderCache.Setup(cache => cache.GetEmlAsync(It.IsAny<int>())).ReturnsAsync((XDocument)null);
        var helper = new MarkdownHelper(
            Mock.Of<IAreaCache>(),
            dataProviderCache.Object,
            Mock.Of<IDatasetRepository>(),
            Mock.Of<IFilterManager>(),
            Mock.Of<ICache<int, ProjectInfo>>(),
            Mock.Of<ICache<int, Taxon>>(),
            Mock.Of<ICache<int, TaxonList>>(),
            Mock.Of<IProcessInfoRepository>(),
            Mock.Of<IProcessedObservationCoreRepository>(),
            Mock.Of<IVocabularyRepository>(),
            NullLogger<MarkdownHelper>.Instance);

        await using var markdown = await helper.GetCitationMarkDownAsync(null, new[] { 8, 1 }, TestContext.Current.CancellationToken);
        Assert.Equal(0, markdown.Position);
        using var reader = new StreamReader(markdown);
        var content = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);

        Assert.Contains("**Referenslista**", content);
        Assert.Contains("Följande dataset har använts: SHARK, Artportalen", content);
        Assert.Contains($"{expectedAuthor} ({DateTime.Now.Year}). SHARK dataset. SMHI.", content);
        Assert.Contains($"Liljeblad J ({DateTime.Now.Year}). Artportalen. SLU Artdatabanken.", content);
    }

    [Fact]
    public async Task GetFilterMarkdownAsync_AllowsFilterWithoutTaxon()
    {
        var filterManager = new Mock<IFilterManager>(MockBehavior.Strict);
        var vocabularyRepository = new Mock<IVocabularyRepository>(MockBehavior.Strict);
        vocabularyRepository.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new List<Vocabulary>());
        var helper = new MarkdownHelper(
            Mock.Of<IAreaCache>(),
            Mock.Of<IDataProviderCache>(),
            Mock.Of<IDatasetRepository>(),
            filterManager.Object,
            Mock.Of<ICache<int, ProjectInfo>>(),
            Mock.Of<ICache<int, Taxon>>(),
            Mock.Of<ICache<int, TaxonList>>(),
            Mock.Of<IProcessInfoRepository>(),
            Mock.Of<IProcessedObservationCoreRepository>(),
            vocabularyRepository.Object,
            NullLogger<MarkdownHelper>.Instance);

        await using var markdown = await helper.GetFilterMarkDownAsync(new SearchFilterBaseDto(), CancellationToken.None);

        Assert.NotNull(markdown);
        Assert.NotEmpty(markdown.ToArray());
        filterManager.VerifyNoOtherCalls();
        vocabularyRepository.Verify(repository => repository.GetAllAsync(), Times.Once);
    }
}
