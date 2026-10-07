using SOS.Api.Contracts.Observations.Requests;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Moq;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Factories;
using SOS.Lib.Managers;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Models.TaxonTree;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;
using Xunit;

namespace SOS.Export.UnitTests.Managers;

/// <summary>
///     Tests for observation manager
/// </summary>
public class TaxonManagerTests
{
    /// <summary>
    ///     Constructor
    /// </summary>
    public TaxonManagerTests()
    {
        _processedTaxonRepositoryMock = new Mock<ITaxonRepository>();
        _taxonListRepositoryMock = new Mock<ITaxonListRepository>();
        _artportalenTaxonRepositoryMock = new Mock<IArtportalenTaxonRepository>();
        _loggerMock = new Mock<ILogger<TaxonManager>>();
        _taxonTreeCache = new ClassCache<Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>>(
            new MemoryCache(new MemoryCacheOptions()),
            new NullLogger<ClassCache<Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>>>());
    }

    private readonly Mock<ITaxonRepository> _processedTaxonRepositoryMock;
    private readonly Mock<ITaxonListRepository> _taxonListRepositoryMock;
    private readonly Mock<IArtportalenTaxonRepository> _artportalenTaxonRepositoryMock;
    private readonly Mock<ILogger<TaxonManager>> _loggerMock;
    private readonly IClassCache<Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>> _taxonTreeCache;

    /// <summary>
    ///     Return object to be tested
    /// </summary>
    private TaxonManager TestObject => CreateTestObject(_taxonTreeCache);

    private TaxonManager CreateTestObject(
        IClassCache<Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>> taxonTreeCache) =>
        new TaxonManager(
            _processedTaxonRepositoryMock.Object,
            _taxonListRepositoryMock.Object,
            taxonTreeCache,
            new ClassCache<TaxonListSetsById>(new MemoryCache(new MemoryCacheOptions()), new NullLogger<ClassCache<TaxonListSetsById>>()),
            _artportalenTaxonRepositoryMock.Object,
            _loggerMock.Object);

    /// <summary>
    ///     Test taxon tree fail
    /// </summary>
    /// <returns></returns>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaxonTreeFail()
    {
        // -----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        _processedTaxonRepositoryMock.Setup(pir => pir.GetAllAsync(It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<BasicTaxon>());

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var tree = await TestObject.GetTaxonTreeAsync();
        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------

        tree.Should().BeNull();
    }

    /// <summary>
    ///     Make a successful taxon tree
    /// </summary>
    /// <returns></returns>
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaxonTreeSuccess()
    {
        // -----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        _processedTaxonRepositoryMock.Setup(pir => pir.GetAllAsync(It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<BasicTaxon>() { new BasicTaxon { Id = 1 } });

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var tree = await TestObject.GetTaxonTreeAsync();
        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------

        tree.Should().NotBe(null);
        tree.TreeNodeById.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ConcurrentTaxonomyRequestsPublishIndependentCacheSnapshots()
    {
        var releaseDyntaxaFetch = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var artportalenFetchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseArtportalenFetch = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        _processedTaxonRepositoryMock
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(),
                It.IsAny<bool>()))
            .Returns(async () =>
            {
                await releaseDyntaxaFetch.Task;
                return new List<BasicTaxon> { new BasicTaxon { Id = 1 } };
            });
        _artportalenTaxonRepositoryMock
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(),
                It.IsAny<bool>()))
            .Returns(async () =>
            {
                artportalenFetchStarted.TrySetResult(true);
                await releaseArtportalenFetch.Task;
                return new List<BasicTaxon> { new BasicTaxon { Id = 2 } };
            });

        var manager = TestObject;
        var dyntaxaTask = manager.GetTaxonTreeAsync(TaxonomyType.Dyntaxa);
        var artportalenTask = manager.GetTaxonTreeAsync(TaxonomyType.Artportalen);

        releaseDyntaxaFetch.SetResult(true);
        await artportalenFetchStarted.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);
        var dyntaxaCacheSnapshot = _taxonTreeCache.Get();
        releaseArtportalenFetch.SetResult(true);

        var trees = await Task.WhenAll(dyntaxaTask, artportalenTask);
        var completeCacheSnapshot = _taxonTreeCache.Get();

        trees[0].TreeNodeById.Should().ContainKey(1);
        trees[1].TreeNodeById.Should().ContainKey(2);
        dyntaxaCacheSnapshot.Should().ContainSingle().Which.Key.Should().Be(TaxonomyType.Dyntaxa);
        completeCacheSnapshot.Should().HaveCount(2);
        completeCacheSnapshot.Should().NotBeSameAs(dyntaxaCacheSnapshot);
        _processedTaxonRepositoryMock.Verify(repository => repository.GetAllAsync(
            It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(),
            It.IsAny<bool>()), Times.Once);
        _artportalenTaxonRepositoryMock.Verify(repository => repository.GetAllAsync(
            It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(),
            It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaxonTreeRefreshFailureIsHandledAndOriginalCacheRemains()
    {
        var originalDyntaxaTree = TaxonTreeFactory.CreateTaxonTree(
            new Dictionary<int, IBasicTaxon>
            {
                [1] = new BasicTaxon { Id = 1 }
            });
        var originalArtportalenTree = TaxonTreeFactory.CreateTaxonTree(
            new Dictionary<int, IBasicTaxon>
            {
                [2] = new BasicTaxon { Id = 2 }
            });
        var originalCache = new Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>
        {
            [TaxonomyType.Dyntaxa] = originalDyntaxaTree,
            [TaxonomyType.Artportalen] = originalArtportalenTree
        };
        var taxonTreeCacheMock = new Mock<IClassCache<Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>>>();
        taxonTreeCacheMock.Setup(cache => cache.Get()).Returns(originalCache);
        _processedTaxonRepositoryMock
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new List<BasicTaxon> { new BasicTaxon { Id = 3 } });
        _artportalenTaxonRepositoryMock
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<ProjectionDefinition<Taxon, BasicTaxon>>(),
                It.IsAny<bool>()))
            .ThrowsAsync(new InvalidOperationException("Taxon repository unavailable"));

        var refreshFailureLogged = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _loggerMock
            .Setup(logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() == "Failed to refresh taxon tree cache"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(new InvocationAction(_ => refreshFailureLogged.TrySetResult(true)));

        _ = CreateTestObject(taxonTreeCacheMock.Object);
        taxonTreeCacheMock.Raise(cache => cache.CacheExpireSoon += null, EventArgs.Empty);

        await refreshFailureLogged.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken);

        taxonTreeCacheMock.Verify(cache => cache.Set(It.IsAny<Dictionary<TaxonomyType, TaxonTree<IBasicTaxon>>>()), Times.Never);
        taxonTreeCacheMock.Object.Get().Should().BeSameAs(originalCache);
        originalCache[TaxonomyType.Dyntaxa].Should().BeSameAs(originalDyntaxaTree);
        originalCache[TaxonomyType.Artportalen].Should().BeSameAs(originalArtportalenTree);
    }
}
