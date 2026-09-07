using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers;

public class MarkdownHelperTests
{
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

        await using var markdown = await helper.GetFilterMarkDownAsync(new SearchFilterDto(), CancellationToken.None);

        Assert.NotNull(markdown);
        Assert.NotEmpty(markdown.ToArray());
        filterManager.VerifyNoOtherCalls();
        vocabularyRepository.Verify(repository => repository.GetAllAsync(), Times.Once);
    }
}
