using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Common.Responses;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Result;
using SOS.Observations.Api.Managers;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class AreaManagerQueryTests
{
    [Fact]
    public async Task AreaQueries_ReturnContractAreasWithMappedBounds()
    {
        var area = new SOS.Lib.Models.Shared.Area(AreaType.Municipality, "123")
        {
            Name = "Test area",
            BoundingBox = new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto(60, 15),
                BottomRight = new LatLonCoordinateDto(59, 16)
            }
        };
        var cache = new Mock<IAreaCache>();
        cache.Setup(x => x.GetAsync(AreaType.Municipality, area.FeatureId)).ReturnsAsync(area);
        cache.Setup(x => x.GetAreasAsync(It.IsAny<IEnumerable<(AreaType, string)>>())).ReturnsAsync([area]);
        cache.Setup(x => x.GetAreasAsync(It.IsAny<IEnumerable<AreaType>>(), "test", 1, 2))
            .ReturnsAsync(new PagedResult<SOS.Lib.Models.Shared.Area>
            {
                Records = [area],
                Skip = 1,
                Take = 2,
                TotalCount = 3
            });
        var manager = new AreaManager(cache.Object, NullLogger<AreaManager>.Instance);

        var single = await manager.GetAreaAsync(AreaTypeDto.Municipality, area.FeatureId);
        var byKeys = await manager.GetAreasAsync([(AreaTypeDto.Municipality, area.FeatureId)]);
        var page = await manager.GetAreasAsync([AreaTypeDto.Municipality], "test", 1, 2);

        AssertArea(single);
        AssertArea(Assert.Single(byKeys));
        Assert.IsType<PagedResultDto<AreaBaseDto>>(page);
        AssertArea(Assert.Single(page.Records));
        Assert.Equal(1, page.Skip);
        Assert.Equal(2, page.Take);
        Assert.Equal(3, page.TotalCount);
    }

    private static void AssertArea(AreaBaseDto area)
    {
        Assert.IsType<AreaBaseDto>(area);
        Assert.Equal(AreaTypeDto.Municipality, area.AreaType);
        Assert.Equal(1, area.AreaTypeId);
        Assert.Equal("123", area.FeatureId);
        Assert.Equal("Test area", area.Name);
        Assert.Equal("Municipality-123", area.Key);
        Assert.IsType<LatLonBoundingBoxDto>(area.BoundingBox);
        Assert.IsType<LatLonCoordinateDto>(area.BoundingBox.TopLeft);
        Assert.Equal(60, area.BoundingBox.TopLeft.Latitude);
        Assert.Equal(15, area.BoundingBox.TopLeft.Longitude);
        Assert.Equal(59, area.BoundingBox.BottomRight.Latitude);
        Assert.Equal(16, area.BoundingBox.BottomRight.Longitude);
    }
}
