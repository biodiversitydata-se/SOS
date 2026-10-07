using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.IntegrationTests.TestData.Factories;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using SOS.Lib.Extensions.Dto;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class GeographicsFilterTests : TestBase
{
    public GeographicsFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task InternalSearch_WithPascalCasePointAndBoundingBox_ReturnsExpectedObservations()
    {
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).HaveCoordinates(14.51874, 58.61502, 10)
            .TheNext(1).HaveCoordinates(14.52874, 58.61502, 10)
            .TheNext(1).HaveCoordinates(16, 60, 10)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        using var apiClient = TestFixture.CreateApiClient();
        const string json = """
            {
              "Geographics": {
                "BoundingBox": {
                  "BottomRight": {
                    "Latitude": 57.868131763328826,
                    "Longitude": 15.792846679687502
                  },
                  "TopLeft": {
                    "Latitude": 59.01794033995248,
                    "Longitude": 13.743896484375002
                  }
                },
                "MaxDistanceFromPoint": 50,
                "Geometries": [
                  {
                    "Type": "Point",
                    "Coordinates": [14.51874, 58.61502],
                    "ConsiderObservationAccuracy": false
                  }
                ],
                "IncludeObservationsOutsideSweden": true
              }
            }
            """;
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await apiClient.PostAsync("/observations/internal/search", content, TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        using var result = JsonDocument.Parse(responseBody);
        result.RootElement.GetProperty("totalCount").GetInt64().Should().Be(1);
        var record = result.RootElement.GetProperty("records").EnumerateArray().Should().ContainSingle().Subject;
        record.GetProperty("location").GetProperty("decimalLongitude").GetDouble().Should().BeApproximately(14.51874, 0.0001);
        record.GetProperty("location").GetProperty("decimalLatitude").GetDouble().Should().BeApproximately(58.61502, 0.0001);
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByMunicipality()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2).HaveAreaFeatureIds("Province1", "County1", "Municipality1")
             .TheNext(1).HaveAreaFeatureIds("Province1", "County1", "Municipality2")
             .TheNext(1).HaveAreaFeatureIds("Province1", "County1", "Municipality3")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithMunicipalityFeatureIds("Municipality1");

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "only two observations are in Municipality1.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByMultipleMunicipalities()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).HaveAreaFeatureIds("Province1", "County1", "Municipality1")
             .TheNext(1).HaveAreaFeatureIds("Province1", "County1", "Municipality2")
             .TheNext(1).HaveAreaFeatureIds("Province1", "County1", "Municipality3")
             .TheNext(1).HaveAreaFeatureIds("Province1", "County1", "Municipality4")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = SearchFilterDtoFactory.CreateWithMunicipalityFeatureIds("Municipality1", "Municipality2");

        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "Municipality1 and Municipality2 are combined with OR.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByProvinceAndMunicipality()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(2).HaveAreaFeatureIds("Province1", "County1", "Municipality1")
             .TheNext(1).HaveAreaFeatureIds("Province1", "County1", "Municipality2")
             .TheNext(1).HaveAreaFeatureIds("Province2", "County2", "Municipality1")
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                Areas = new[] {
                    new AreaFilterDto { AreaType = AreaTypeDto.Province, FeatureId = "Province1" },
                    new AreaFilterDto { AreaType = AreaTypeDto.Municipality, FeatureId = "Municipality1" }
                }
            }
        };
       
        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "only two observations match both Province1 AND Municipality1.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPolygon()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).HaveCoordinates(11.50004, 68.00003, 10)
            .TheNext(1).HaveCoordinates(11.50006, 68.00005, 10)
            .TheNext(1).HaveCoordinates(11.00005, 68.00004, 10)
            .TheNext(1).HaveCoordinates(11.50005, 68.50005, 10)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        using var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                Geometries = new[] {
                    new Point(11.50005, 68.00004).ToCircle(10).ToGeoJsonGeometry()
                }
            }
        };

        // Both validation modes use the same observations, without processing and indexing them twice.
        foreach (var validateSearchFilter in new[] { false, true })
        {
            using var content = JsonContent.Create(searchFilter, null, JsonSerializerOptions);
            using var response = await apiClient.PostAsync($"/observations/search?validateSearchFilter={validateSearchFilter}", content, TestContext.Current.CancellationToken);
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"validateSearchFilter={validateSearchFilter}");
            result!.TotalCount.Should().Be(2,
                because: $"only two observations are inside the polygon, with validateSearchFilter={validateSearchFilter}.");
        }
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPolygonAndMaxAccuracy()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).HaveCoordinates(11.50004, 68.00003, 10)
             .TheNext(1).HaveCoordinates(11.50006, 68.00005, 10)
             .TheNext(1).HaveCoordinates(11.50005, 68.00004, 20)
             .TheNext(1).HaveCoordinates(11.50005, 69.00005, 10)
            .Build();
        var apiClient = TestFixture.CreateApiClient();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                Geometries = new[] {
                    new Point(11.50005, 68.00004).ToCircle(10).ToGeoJsonGeometry()
                },
                MaxAccuracy = 15
            }
        };

        // Act            
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "only two observations are inside the polygon and have accuracy <= 15 meters.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPolygonAndConsiderAccuracy()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).HaveCoordinates(11.50005, 68.00004, 10)
             .TheNext(1).HaveCoordinates(11.505, 68.00004, 1000)
             .TheNext(1).HaveCoordinates(12.00005, 68.00004, 10)
             .TheNext(1).HaveCoordinates(14.5, 56.00005, 10)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                Geometries = new[] {
                    new Point(11.50005, 68.00004).ToCircle(10).ToGeoJsonGeometry()
                },
                ConsiderObservationAccuracy = true
            }
        };

        // Act            
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "two observations intersect the circle polygon used in the filter " +
                     "when observation accuracy is considered");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPolygonAndConsiderDisturbanceRadius()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).HaveCoordinates(11.50005, 68.00004, 10)
                         .With(o => o.TaxonId = 100011) // Disturbance radius: 2000m
             .TheNext(1).HaveCoordinates(11.505, 68.00004, 10)
                         .With(o => o.TaxonId = 100009) // Disturbance radius: 1000m
             .TheNext(1).HaveCoordinates(12.00005, 68.00004, 10)
                         .With(o => o.TaxonId = 102933) // Disturbance radius: 0m
             .TheNext(1).HaveCoordinates(14.5, 56.00005, 10)
                         .With(o => o.TaxonId = 100009) // Disturbance radius: 1000m
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                Geometries = new[] {
                    new Point(11.50005, 68.00004).ToCircle(10).ToGeoJsonGeometry()
                },
                ConsiderDisturbanceRadius = true
            }
        };
        var apiClient = TestFixture.CreateApiClient();

        // Act            
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "two observations intersect the circle polygon used in the filter " +
                     "when disturbance radius is considered");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByBoundingBox()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange - Create verbatim observations
        //-----------------------------------------------------------------------------------------------------------            
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).HaveCoordinates(11.50004, 68.00003, 10)
            .TheNext(1).HaveCoordinates(11.50006, 68.00005, 10)
            .TheNext(1).HaveCoordinates(12.00005, 68.00004, 10)
            .TheNext(1).HaveCoordinates(14.5, 56.00005, 10)
            .Build();
        var apiClient = TestFixture.CreateApiClient();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                BoundingBox = new LatLonBoundingBoxDto
                {
                    BottomRight = new LatLonCoordinateDto { Latitude = 68.00001, Longitude = 11.50009 },
                    TopLeft = new LatLonCoordinateDto { Latitude = 68.00009, Longitude = 11.50001 }
                }
            }
        };

        // Act            
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "only two observations are inside the bounding box.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByMunicipalityPolygon()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        var uppsala = Geometries.UppsalaMunicipality();
        var inside = uppsala.InteriorPoint;
        var outside = new Point(uppsala.EnvelopeInternal.MaxX + 0.5, uppsala.EnvelopeInternal.MinY - 0.5);
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
           .All().HaveValuesFromPredefinedObservations()
           .TheFirst(1).HaveCoordinates(inside.X, inside.Y, 10)
            .TheNext(1).HaveCoordinates(inside.X, inside.Y, 1000)
            .TheNext(2).HaveCoordinates(outside.X, outside.Y, 10)
           .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                Geometries = [uppsala.ToGeoJsonGeometry()]
            }
        };
        // Act
        var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, options: JsonSerializerOptions), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "only two observations are in the municipality Uppsala.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_ConsiderAuthorizationBuffer()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        const int bufferSize = 1000;
        var uppsala = Geometries.UppsalaMunicipality();
        var uppsalaWithBuffer = uppsala.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM).Buffer(bufferSize).Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84);
        var buffer = uppsalaWithBuffer.Difference(uppsala);
        var inside = uppsala.InteriorPoint;
        var insideBuffer = buffer.InteriorPoint;
        var outside = new Point(uppsalaWithBuffer.EnvelopeInternal.MaxX + 0.5, uppsalaWithBuffer.EnvelopeInternal.MinY - 0.5);
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All()
                .HaveValuesFromPredefinedObservations()
                .HaveTaxonSensitivityCategory(4)
            .TheFirst(1).HaveCoordinates(inside.X, inside.Y, 10)
            .TheNext(1).HaveCoordinates(insideBuffer.X, insideBuffer.Y, 10)
            .TheNext(2).HaveCoordinates(outside.X, outside.Y, 10)
            .Build();
        
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithMunicipalitySightingIndicationAuthority(maxProtectionLevel: 5, "380"/*Uppsala*/, bufferSize);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                ConsiderAuthorizationBuffer = true
            }
        };
        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter, options: JsonSerializerOptions), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: $"two observations are in Uppsala or max {bufferSize}m outside.");
    }

    [Fact]
    public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_DontConsiderAuthorizationBuffer()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------
        const int bufferSize = 1000;
        var uppsala = Geometries.UppsalaMunicipality();
        var uppsalaWithBuffer = uppsala.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM).Buffer(bufferSize).Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84);
        var buffer = uppsalaWithBuffer.Difference(uppsala);
        var inside = uppsala.InteriorPoint;
        var insideBuffer = buffer.InteriorPoint;
        var outside = new Point(uppsalaWithBuffer.EnvelopeInternal.MaxX + 0.5, uppsalaWithBuffer.EnvelopeInternal.MinY - 0.5);
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(4)
            .All()
                .HaveValuesFromPredefinedObservations()
                .HaveTaxonSensitivityCategory(4)
            .TheFirst(1).HaveCoordinates(inside.X, inside.Y, 10).HaveAreaFeatureIds("Province1", "County1", "380") // Uppsala
            .TheNext(1).HaveCoordinates(insideBuffer.X, insideBuffer.Y, 10).HaveAreaFeatureIds("Province1", "County1", "381") // Enköping
            .TheNext(2).HaveCoordinates(outside.X, outside.Y, 10).HaveAreaFeatureIds("Province2", "County2", "382") // Östhammar
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithMunicipalitySightingIndicationAuthority(maxProtectionLevel: 5, "380"/*Uppsala*/, bufferSize);
        var apiClient = TestFixture.CreateApiClientWithReplacedService(userServiceStub);
        var searchFilter = new SearchFilterDto
        {
            Geographics = new GeographicsFilterDto
            {
                ConsiderAuthorizationBuffer = false
            }
        };
        // Act
        var response = await apiClient.PostAsync($"/observations/search?sensitiveObservations=true", JsonContent.Create(searchFilter, options: JsonSerializerOptions), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(1,
            because: "only one observation is in Uppsala, without including the authorization buffer.");
    }
}
