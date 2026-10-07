using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using FluentAssertions;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Result;
using SOS.Api.Contracts.Aggregations.Responses;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class GeoExtensionsTests
{
    [Theory]
    [InlineData(CoordinateSys.WebMercator)]
    [InlineData(CoordinateSys.Rt90_25_gon_v)]
    [InlineData(CoordinateSys.SWEREF99)]
    [InlineData(CoordinateSys.SWEREF99_TM)]
    [InlineData(CoordinateSys.WGS84)]
    [InlineData(CoordinateSys.ETRS89)]
    [InlineData(CoordinateSys.ETRS89_LAEA_Europe)]
    public void CoordinateTransform_RoundTripsAllSharedSystemsAndPreservesSrid(CoordinateSys coordinateSystem)
    {
        var point = new Point(18, 59) { SRID = 4326 };

        var projected = point.Transform(CoordinateSys.WGS84, coordinateSystem, usePointCache: false);
        var restored = projected.Transform(coordinateSystem, CoordinateSys.WGS84, usePointCache: false);

        projected.SRID.Should().Be((int)coordinateSystem);
        restored.SRID.Should().Be(4326);
        // Projected systems round to metres, so the inverse has metre-scale precision.
        restored.X.Should().BeApproximately(point.X, 0.00002);
        restored.Y.Should().BeApproximately(point.Y, 0.00002);
    }

    [Fact]
    public void GetCompleteBoundingBox_NullBoundingBox_ReturnsNull()
    {
        LatLonBoundingBoxDto? bbox = null;

        var result = bbox.GetCompleteBoundingBox();

        result.Should().BeNull();
    }

    [Fact]
    public void ToXYBoundingBoxDto_NullBoundingBox_ReturnsNull()
    {
        LatLonBoundingBoxDto? bbox = null;

        var result = bbox.ToXYBoundingBoxDto();

        result.Should().BeNull();
    }

    [Fact]
    public void GetCompleteBoundingBox_ValidBoundingBox_ReusesCoordinates()
    {
        var bbox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto(60.0, 15.0),
            BottomRight = new LatLonCoordinateDto(55.0, 20.0)
        };

        var result = bbox.GetCompleteBoundingBox();

        result.Should().BeSameAs(bbox);
        result!.TopLeft.Latitude.Should().Be(60.0);
        result.TopLeft.Longitude.Should().Be(15.0);
        result.BottomRight.Latitude.Should().Be(55.0);
        result.BottomRight.Longitude.Should().Be(20.0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IncompleteBoundingBox_RemainsIgnoredBySearchAndEnvelope(bool missingTopLeft)
    {
        var box = new LatLonBoundingBoxDto
        {
            TopLeft = missingTopLeft ? null! : new LatLonCoordinateDto(60, 15),
            BottomRight = missingTopLeft ? new LatLonCoordinateDto(55, 20) : null!
        };
        var request = new SearchFilterInternalDto { Geographics = new() { BoundingBox = box } };

        box.GetCompleteBoundingBox().Should().BeNull();
        box.ToEnvelope().Should().BeNull();
        request.ToSearchFilterInternal(0, "sv-SE").Location.Geometries.BoundingBox.Should().BeNull();
    }

    [Fact]
    public void SearchFilter_ReusesCompleteBoundingBoxWithoutMapping()
    {
        var box = LatLonBoundingBoxDto.Create(15, 60, 20, 55);
        var request = new SearchFilterInternalDto { Geographics = new() { BoundingBox = box } };

        var searchFilter = request.ToSearchFilterInternal(0, "sv-SE");

        searchFilter.Location.Geometries.BoundingBox.Should().BeSameAs(box);
        box.ToEnvelope().Should().BeEquivalentTo(new Envelope(15, 20, 55, 60));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BoundingBox_GetPolygonPreservesCoordinatesAndRingOrientation(bool counterClockwise)
    {
        var box = LatLonBoundingBoxDto.Create(15, 60, 20, 55);

        var polygon = box.GetPolygon(counterClockwiseOrder: counterClockwise);

        ((LinearRing)polygon.ExteriorRing).IsCCW.Should().Be(counterClockwise);
        polygon.EnvelopeInternal.Should().BeEquivalentTo(new Envelope(15, 20, 55, 60));
        polygon.Coordinates.Should().HaveCount(5);
        polygon.Coordinates[0].Should().BeEquivalentTo(new Coordinate(15, 60));
        polygon.Coordinates[^1].Should().BeEquivalentTo(polygon.Coordinates[0]);
        box.GetPolygon(CoordinateSys.SWEREF99_TM, counterClockwise).SRID.Should().Be(3006);
    }

    [Fact]
    public void MetricGrid_ToDto_UsesContractModelsAndPreservesBoundsAndCounts()
    {
        var source = new GeoGridMetricResult
        {
            BoundingBox = new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto(60, 15),
                BottomRight = new LatLonCoordinateDto(55, 20)
            },
            GridCellCount = 1,
            GridCellSizeInMeters = 1000,
            GridCells = [new GridCell
            {
                ObservationsCount = 7,
                TaxaCount = 3,
                MetricBoundingBox = new XYBoundingBox
                {
                    TopLeft = new XYCoordinate(500000, 7000000),
                    BottomRight = new XYCoordinate(501000, 6999000)
                }
            }]
        };
        var result = source.ToDto();

        result.Should().BeOfType<GeoGridMetricResultDto>();
        result.GridCells.Should().ContainSingle();
        result.GridCells.Single().Should().BeOfType<GridCellDto>();
        result.GridCells.Single().ObservationsCount.Should().Be(7);
        result.GridCells.Single().TaxaCount.Should().Be(3);
        result.GridCells.Single().MetricBoundingBox.TopLeft.X.Should().Be(500000);
        result.BoundingBox.TopLeft.Latitude.Should().Be(60);
        result.BoundingBox.Should().BeSameAs(source.BoundingBox);
    }

    [Fact]
    public void PagedResult_ToDto_MaterializesRecordsInContractList()
    {
        var source = new PagedResult<int> { Skip = 2, Take = 2, TotalCount = 4 };

        var result = source.ToPagedResultDto(Enumerable.Range(3, 2));

        result.Should().BeOfType<PagedResultDto<int>>();
        result.Records.Should().Equal(3, 4);
        result.Skip.Should().Be(2);
        result.Take.Should().Be(2);
        result.TotalCount.Should().Be(4);
    }

    [Fact]
    public void AdaptiveSearchResultFactory_MapsServerGeometryToContractGeoJson()
    {
        var boundingBox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto { Latitude = 60, Longitude = 15 },
            BottomRight = new LatLonCoordinateDto { Latitude = 55, Longitude = 20 }
        };
        var area = new Area(AreaType.Municipality, "123")
        {
            Name = "Test area",
            BoundingBox = new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto(60, 15),
                BottomRight = new LatLonCoordinateDto(55, 20)
            }
        };
        var point = new GeometryFactory().CreatePoint(new Coordinate(18, 59));

        var result = AdaptiveSearchResultFactory.CreateObservationsResult(
            boundingBox,
            totalObservationsCount: 1,
            observations: new[] { "observation" },
            geographicAreas: [(area, point)]);

        result.Should().BeOfType<AdaptiveSearchResultDto<string>>();
        result.BoundingBox!.TopLeft.Latitude.Should().Be(60);
        result.FilterAreas.Should().ContainSingle();
        var geometry = result.FilterAreas!.Single().Geometry.Should().BeOfType<GeoJSON.Text.Geometry.Point>().Subject;
        geometry.Coordinates.Longitude.Should().Be(18);
        geometry.Coordinates.Latitude.Should().Be(59);
    }
}
