using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using NetTopologySuite.Geometries;
using SOS.Api.Client.Clients;
using SOS.Api.Client.Requests;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.AnalysisApi;


[Collection(TestCollection.Name)]
public class AooEooEndpointTests : ElasticsearchTestBase
{
    private const int AccuracyInMeters = 10;
    private const int ExpectedGridCellAreaInKm2 = 4;
    private const int GridCellSizeInMeters = 2000;
    private const int TaxonId = 100012;

    public AooEooEndpointTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task CalculateAooAndEooInternalEndpoint_ReturnsExpectedAoo_WhenObservationsAreFarApart()
    {
        // Arrange
        var points = new[]
        {
            ToWgs84Point(300100, 6200100),
            ToWgs84Point(674100, 6582100),
            ToWgs84Point(800100, 7300100)
        };

        // Act
        using var result = await CalculateAooAndEooAsync(points);
        var eooProperties = GetEooProperties(result);

        // Assert
        AssertEooGeometryIsPolygon(result);
        GetInt32(eooProperties, "aoo").Should().Be(12);
        GetInt32(eooProperties, "eoo").Should().BeGreaterThan(12);
        GetInt32(eooProperties, "gridCellArea").Should().Be(ExpectedGridCellAreaInKm2);
        GetInt32(eooProperties, "observationsCount").Should().Be(3);
        GetFeatureCount(result).Should().Be(1);
    }

    [Fact]
    public async Task CalculateAooAndEooInternalEndpoint_ReturnsOneGridCell_WhenAllObservationsAreInSameGridCell()
    {
        // Arrange
        var points = new[]
        {
            ToWgs84Point(674100, 6582100),
            ToWgs84Point(674900, 6582900),
            ToWgs84Point(675700, 6583700)
        };

        // Act
        using var result = await CalculateAooAndEooAsync(points, returnGridCells: true);
        var eooProperties = GetEooProperties(result);
        var gridCellProperties = GetGridCellProperties(result);

        // Assert
        AssertEooGeometryIsPolygon(result);
        GetInt32(eooProperties, "aoo").Should().Be(4);
        GetInt32(eooProperties, "gridCellArea").Should().Be(ExpectedGridCellAreaInKm2);
        GetInt32(eooProperties, "observationsCount").Should().Be(3);

        gridCellProperties.Should().ContainSingle();
        GetString(gridCellProperties[0], "id").Should().Be("2kmN6582E674");
        GetInt64(gridCellProperties[0], "observationsCount").Should().Be(3);
    }

    [Fact]
    public async Task CalculateAooAndEooInternalEndpoint_ReturnsOnlyOccupiedGridCells_WhenNorthSouthGridCellsHaveEmptyCellsBetween()
    {
        // Arrange
        var points = new[]
        {
            ToWgs84Point(674100, 6582100),
            ToWgs84Point(674100, 6586100),
            ToWgs84Point(674100, 6590100)
        };

        // Act
        using var result = await CalculateAooAndEooAsync(points, returnGridCells: true);
        var eooProperties = GetEooProperties(result);
        var gridCellProperties = GetGridCellProperties(result);

        // Assert
        AssertEooGeometryIsPolygon(result);
        GetInt32(eooProperties, "aoo").Should().Be(12);
        GetInt32(eooProperties, "gridCellArea").Should().Be(ExpectedGridCellAreaInKm2);
        GetInt32(eooProperties, "observationsCount").Should().Be(3);

        gridCellProperties.Select(properties => GetString(properties, "id"))
            .Should().BeEquivalentTo(new[]
            {
                "2kmN6582E674",
                "2kmN6586E674",
                "2kmN6590E674"
            });
        gridCellProperties.Select(properties => GetInt64(properties, "observationsCount"))
            .Should().AllBeEquivalentTo(1);
    }

    [Fact]
    public async Task SosAnalysisClient_CalculateAooEooAsync_UsesAnalysisApiPipeline()
    {
        var points = new[]
        {
            ToWgs84Point(674100, 6582100),
            ToWgs84Point(674900, 6582900),
            ToWgs84Point(675700, 6583700)
        };
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(CreateVerbatimObservations(points));
        var client = new SosAnalysisClient(TestFixture.CreateAnalysisApiClient());
        var filter = new SearchFilterAggregationInternalDto
        {
            Geographics = new GeographicsFilterDto
            {
                BoundingBox = CreateBoundingBox(points)
            }
        };

        var result = await client.CalculateAooEooAsync(
            filter,
            new AooEooOptions
            {
                ValidateFilter = false,
                AlphaValues = [1],
                GridCellSizeInMeters = GridCellSizeInMeters,
                ReturnGridCells = true
            },
            TestContext.Current.CancellationToken);

        result.Features.Should().NotBeEmpty();
        result.Features
            .Where(feature => feature.Properties.ContainsKey("id"))
            .Select(feature => feature.Properties["id"]?.ToString())
            .Should().Contain("eoo-1");
    }

    [Fact]
    public async Task SosAnalysisClient_AggregateAreasAsync_UsesAnalysisApiPipeline()
    {
        var points = new[]
        {
            ToWgs84Point(674100, 6582100),
            ToWgs84Point(674900, 6582900)
        };
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(CreateVerbatimObservations(points));
        var client = new SosAnalysisClient(TestFixture.CreateAnalysisApiClient());
        var filter = new SearchFilterAggregationInternalDto
        {
            Geographics = new GeographicsFilterDto
            {
                BoundingBox = CreateBoundingBox(points)
            }
        };

        var result = await client.AggregateAreasAsync(
            filter,
            new AreaAggregationOptions
            {
                AreaType = AreaTypeAggregate.Province,
                ValidateFilter = false
            },
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Features.Should().NotBeNull();
    }

    [Fact]
    public async Task SosAnalysisClient_CalculateArticle17Async_UsesAnalysisApiPipeline()
    {
        var points = new[]
        {
            ToWgs84Point(674100, 6582100),
            ToWgs84Point(674900, 6582900),
            ToWgs84Point(675700, 6583700)
        };
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(CreateVerbatimObservations(points));
        var client = new SosAnalysisClient(TestFixture.CreateAnalysisApiClient());
        var filter = new SearchFilterAggregationInternalDto
        {
            Geographics = new GeographicsFilterDto
            {
                BoundingBox = CreateBoundingBox(points)
            }
        };

        var result = await client.CalculateArticle17Async(
            filter,
            new Article17Options
            {
                ValidateFilter = false,
                MaxDistance = 25_000,
                GridCellSizeInMeters = GridCellSizeInMeters
            },
            TestContext.Current.CancellationToken);

        result.Should().NotBeNull();
        result.Features.Should().NotBeNull();
    }

    private async Task<JsonDocument> CalculateAooAndEooAsync(IReadOnlyCollection<Point> points, bool returnGridCells = false)
    {
        var verbatimObservations = CreateVerbatimObservations(points);
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

        var apiClient = TestFixture.CreateAnalysisApiClient();
        var searchFilter = new SearchFilterAggregationInternalDto
        {
            Geographics = new GeographicsFilterDto
            {
                BoundingBox = CreateBoundingBox(points)
            }
        };

        var response = await apiClient.PostAsync(
            CreateAooEooUrl(returnGridCells),
            JsonContent.Create(searchFilter, null, JsonSerializerOptions),
            TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK, "response body was {0}", responseBody);
        responseBody.Should().NotBe("null");

        return JsonDocument.Parse(responseBody);
    }

    private static string CreateAooEooUrl(bool returnGridCells)
    {
        var url =
            "/internal/aoo_eoo?validateFilter=false" +
            "&alphaValues=1" +
            $"&gridCellSizeInMeters={GridCellSizeInMeters}" +
            "&useCenterPoint=true" +
            "&useEdgeLengthRatio=true" +
            "&allowHoles=false" +
            $"&returnGridCells={returnGridCells.ToString().ToLowerInvariant()}" +
            "&includeEmptyCells=false" +
            "&metricCoordinateSys=3006" +
            "&coordinateSystem=4326";

        return url;
    }

    private static IList<ArtportalenObservationVerbatim> CreateVerbatimObservations(IReadOnlyCollection<Point> points)
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(points.Count)
            .All().HaveValuesFromPredefinedObservations()
            .Build();

        for (var i = 0; i < points.Count; i++)
        {
            var point = points.ElementAt(i);
            var observation = observations[i];
            var webMercatorPoint = point.Transform(CoordinateSys.WGS84, CoordinateSys.WebMercator);

            observation.TaxonId = TaxonId;
            observation.Site.XCoord = (int)webMercatorPoint.Coordinate.X;
            observation.Site.YCoord = (int)webMercatorPoint.Coordinate.Y;
            observation.Site.Point = point.ToGeoJson();
            observation.Site.PointWithBuffer = point.ToCircle(AccuracyInMeters).ToGeoJson();
            observation.Site.Accuracy = AccuracyInMeters;
        }

        return observations;
    }

    private static LatLonBoundingBoxDto CreateBoundingBox(IEnumerable<Point> points)
    {
        const double margin = 0.02;
        var pointArray = points.ToArray();

        return new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto
            {
                Latitude = pointArray.Max(point => point.Y) + margin,
                Longitude = pointArray.Min(point => point.X) - margin
            },
            BottomRight = new LatLonCoordinateDto
            {
                Latitude = pointArray.Min(point => point.Y) - margin,
                Longitude = pointArray.Max(point => point.X) + margin
            }
        };
    }

    private static Point ToWgs84Point(double sweref99TmX, double sweref99TmY)
    {
        return new Point(sweref99TmX, sweref99TmY)
            .Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, usePointCache: false);
    }

    private static JsonElement GetEooProperties(JsonDocument document)
    {
        return GetEooFeature(document)
            .GetProperty("properties");
    }

    private static JsonElement GetEooFeature(JsonDocument document)
    {
        return document.RootElement
            .GetProperty("features")
            .EnumerateArray()
            .Single(feature => GetString(feature.GetProperty("properties"), "id") == "eoo-1");
    }

    private static List<JsonElement> GetGridCellProperties(JsonDocument document)
    {
        return document.RootElement
            .GetProperty("features")
            .EnumerateArray()
            .Select(feature => feature.GetProperty("properties"))
            .Where(properties => properties.TryGetProperty("taxaCount", out _))
            .ToList();
    }

    private static int GetFeatureCount(JsonDocument document)
    {
        return document.RootElement.GetProperty("features").GetArrayLength();
    }

    private static void AssertEooGeometryIsPolygon(JsonDocument document)
    {
        GetString(GetEooFeature(document).GetProperty("geometry"), "type").Should().Be("Polygon");
    }

    private static int GetInt32(JsonElement element, string propertyName)
    {
        return element.GetProperty(propertyName).GetInt32();
    }

    private static long GetInt64(JsonElement element, string propertyName)
    {
        return element.GetProperty(propertyName).GetInt64();
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.GetProperty(propertyName).GetString()!;
    }
}
