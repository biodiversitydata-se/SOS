using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Repositories.Interfaces;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class PreparedObservationFilterTests
{
    [Fact]
    public async Task PreparedQueriesReuseFilterAndLegacyQueriesStillPrepareIt()
    {
        var filters = new Mock<IFilterManager>(MockBehavior.Strict);
        var repository = new Mock<IProcessedObservationRepository>();
        var filter = new SearchFilterInternal(123, ProtectionFilter.Public);
        filters.Setup(m => m.PrepareFilterAsync(42, "app", filter, "Sighting", 0, false, false, true, false))
            .Callback(() => filter.RoleId = 42)
            .Returns(Task.CompletedTask);
        repository.Setup(r => r.GetCountAndExtentAsync(filter, false, true))
            .ReturnsAsync((0L, (LatLonBoundingBox?)null, 0L));
        repository.Setup(r => r.GetChunkAsync<JsonObject>(filter, 0, 10,
                It.Is<SearchExecutionOptions>(o => o.KnownTotalCount == 0L)))
            .ReturnsAsync(new PagedResult<JsonObject> { Records = Array.Empty<JsonObject>() });
        repository.Setup(r => r.GetLocationRepresentativeChunkAsync<JsonObject>(filter, 10, "location.locationId", false))
            .ReturnsAsync(new LocationRepresentativeResult<JsonObject> { Records = Array.Empty<JsonObject>() });
        repository.Setup(r => r.GetGeogridTileAggregationAsync(filter, 8)).ReturnsAsync(new GeoGridTileResult());
        var manager = CreateManager(repository.Object, filters.Object);

        var prepared = await manager.PrepareObservationFilterAsync(42, "app", filter);
        await manager.GetCountAndExtentAsync(prepared);
        var options = new SearchExecutionOptions { KnownTotalCount = 0L };
        await manager.GetChunkAsync(prepared, 0, 10, options);
        await manager.GetLocationRepresentativeChunkAsync(prepared, 10, "location.locationId");
        await manager.GetGeogridTileAggregationAsync(prepared, 8);

        Assert.Same(filter, prepared.Filter);
        Assert.Equal(42, prepared.Filter.RoleId);
        filters.Verify(m => m.PrepareFilterAsync(42, "app", filter, "Sighting", 0, false, false, true, false), Times.Once);
        repository.Verify(r => r.GetCountAndExtentAsync(filter, false, true), Times.Once);
        repository.Verify(r => r.GetChunkAsync<JsonObject>(filter, 0, 10, options), Times.Once);
        repository.Verify(r => r.GetLocationRepresentativeChunkAsync<JsonObject>(filter, 10, "location.locationId", false), Times.Once);
        repository.Verify(r => r.GetGeogridTileAggregationAsync(filter, 8), Times.Once);

        await manager.GetCountAndExtentAsync(42, "app", filter);
        await manager.GetChunkAsync(42, "app", filter, 0, 10, options);
        await manager.GetLocationRepresentativeChunkAsync(42, "app", filter, 10, "location.locationId");
        await manager.GetGeogridTileAggregationAsync(42, "app", filter, 8);
        filters.Verify(m => m.PrepareFilterAsync(42, "app", filter, "Sighting", 0, false, false, true, false), Times.Exactly(5));
    }

    [Fact]
    public async Task PreparedCountCanSkipExtent()
    {
        var filters = new Mock<IFilterManager>();
        var repository = new Mock<IProcessedObservationRepository>();
        var filter = new SearchFilterInternal(123, ProtectionFilter.Public);
        repository.Setup(r => r.GetCountAndExtentAsync(filter, false, false))
            .ReturnsAsync((123L, (LatLonBoundingBox?)null, 12L));
        var manager = CreateManager(repository.Object, filters.Object);

        var result = await manager.GetCountAndExtentAsync(new PreparedObservationFilter(filter), includeExtent: false);

        Assert.Equal(123L, result.Count);
        Assert.Equal(12L, result.EstimatedLocationCount);
        Assert.Null(result.Extent);
        repository.Verify(r => r.GetCountAndExtentAsync(filter, false, false), Times.Once);
        filters.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task FailedAuthorizationDoesNotProducePreparedFilterOrQueryRepository()
    {
        var filters = new Mock<IFilterManager>(MockBehavior.Strict);
        var repository = new Mock<IProcessedObservationRepository>();
        var filter = new SearchFilterInternal(123, ProtectionFilter.Sensitive);
        filters.Setup(m => m.PrepareFilterAsync(42, "app", filter, "Sighting", 0, false, false, true, false))
            .ThrowsAsync(new AuthenticationRequiredException("Role denied"));
        var manager = CreateManager(repository.Object, filters.Object);
        repository.Invocations.Clear();

        await Assert.ThrowsAsync<AuthenticationRequiredException>(() => manager.PrepareObservationFilterAsync(42, "app", filter));
        repository.VerifyNoOtherCalls();
    }

    private static ObservationManager CreateManager(IProcessedObservationRepository repository, IFilterManager filters)
        => new(repository, Mock.Of<IProtectedLogRepository>(), Mock.Of<IVocabularyValueResolver>(), filters,
            new HttpContextAccessor(), Mock.Of<ITaxonObservationCountCache>(), Mock.Of<IGeneralizationResolver>(),
            Mock.Of<IDataProviderCache>(), NullLogger<ObservationManager>.Instance);
}
