using FizzWare.NBuilder;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.ContainerIntegrationTests.Setup;
using SOS.ContainerIntegrationTests.TestData.TestDataBuilder;
using SOS.ContainerIntegrationTests.TestData.Factories;

namespace SOS.ContainerIntegrationTests.Tests.ObservationsBySearchEndpoint
{
    [Collection(TestCollection.Name)]
    public class GeographicsFilterTests : TestBase
    {
        public GeographicsFilterTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
        {
        }

        [Fact]
        public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByMunicipality()
        {
            // Arrange
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All().HaveValuesFromPredefinedObservations()
                .TheFirst(60).HaveAreaFeatureIds("Province1", "County1", "Municipality1")
                 .TheNext(20).HaveAreaFeatureIds("Province1", "County1", "Municipality2")
                 .TheNext(20).HaveAreaFeatureIds("Province1", "County1", "Municipality3")
                .Build();
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var apiClient = TestFixture.CreateApiClient();
            var searchFilter = SearchFilterDtoFactory.CreateWithMunicipalityFeatureIds("Municipality1");

            // Act
            var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.TotalCount.Should().Be(60,
                because: "60 observations added to Elasticsearch are in the municipality Municipality1.");
        }

        [Fact]
        public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByMultipleMunicipalities()
        {
            // Arrange
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All().HaveValuesFromPredefinedObservations()
                .TheFirst(30).HaveAreaFeatureIds("Province1", "County1", "Municipality1")
                 .TheNext(30).HaveAreaFeatureIds("Province1", "County1", "Municipality2")
                 .TheNext(20).HaveAreaFeatureIds("Province1", "County1", "Municipality3")
                 .TheNext(20).HaveAreaFeatureIds("Province1", "County1", "Municipality4")
                .Build();
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var apiClient = TestFixture.CreateApiClient();
            var searchFilter = SearchFilterDtoFactory.CreateWithMunicipalityFeatureIds("Municipality1", "Municipality2");

            // Act
            var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.TotalCount.Should().Be(60,
                because: "60 observations added to Elasticsearch are in the municipality Municipality1 OR Municipality2.");
        }

        [Fact]
        public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByProvinceAndMunicipality()
        {
            // Arrange
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All().HaveValuesFromPredefinedObservations()
                .TheFirst(60).HaveAreaFeatureIds("Province1", "County1", "Municipality1")
                 .TheNext(20).HaveAreaFeatureIds("Province1", "County1", "Municipality2")
                 .TheNext(20).HaveAreaFeatureIds("Province1", "County1", "Municipality3")
                .Build();
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var apiClient = TestFixture.CreateApiClient();
            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Areas = new[] {
                        new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Province, FeatureId = "Province1" },
                        new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "Municipality1" }
                    }
                }
            };

            // Act
            var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter));
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.TotalCount.Should().Be(60,
                because: "60 observations added to Elasticsearch are in the province Province1 AND in Municipality1.");
        }

        [Fact]
        public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPolygon()
        {
            // Arrange
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All().HaveValuesFromPredefinedObservations()
                .TheFirst(60).HaveCoordinatesInSpan(1.00001, 1.00009, 1.00001, 1.00009, 10)
                .TheNext(20).HaveCoordinatesInSpan(2.00001, 2.00009, 1.00001, 1.00009, 10)
                .TheNext(20).HaveCoordinatesInSpan(1.00001, 1.00009, 2.00001, 2.00009, 10)
                .Build();
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var apiClient = TestFixture.CreateApiClient();
            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new[] {
                        new Point(1.00005, 1.00004).ToCircle(10).ToGeoShape()
                    }
                }
            };

            // Act            
            var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions));
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.TotalCount.Should().Be(60,
                because: "60 observations added to Elasticsearch are located inside the circle polygon used in the filter.");
        }

        [Fact]
        public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPolygonAndMaxAccuracy()
        {
            // Arrange
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All().HaveValuesFromPredefinedObservations()
                .TheFirst(60).HaveCoordinatesInSpan(1.00001, 1.00009, 1.00001, 1.00009, 10)
                 .TheNext(20).HaveCoordinatesInSpan(1.00001, 1.00009, 1.00001, 1.00009, 20)
                 .TheNext(20).HaveCoordinatesInSpan(1.00001, 1.00009, 2.00001, 2.00009, 10)
                .Build();
            var apiClient = TestFixture.CreateApiClient();
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new[] {
                        new Point(1.00005, 1.00004).ToCircle(10).ToGeoShape()
                    },
                    MaxAccuracy = 15
                }
            };

            // Act            
            var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions));
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.TotalCount.Should().Be(60,
                because: "60 observations added to Elasticsearch are located inside the circle polygon used in the filter " +
                         "and also with accuracy <= 15 meter.");
        }

        [Fact]
        public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPolygonAndConsiderAccuracy()
        {
            // Arrange
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All().HaveValuesFromPredefinedObservations()
                .TheFirst(30).HaveCoordinatesInSpan(1.00001, 1.00009, 1.00001, 1.00009, 10)
                 .TheNext(30).HaveCoordinatesInSpan(1.00100, 1.00900, 1.00001, 1.00009, 1000)
                 .TheNext(20).HaveCoordinatesInSpan(2.00001, 2.00009, 1.00001, 1.00009, 10)
                 .TheNext(20).HaveCoordinatesInSpan(1.00001, 1.00009, 2.00001, 2.00009, 10)
                .Build();
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var apiClient = TestFixture.CreateApiClient();
            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new[] {
                        new Point(1.00005, 1.00004).ToCircle(10).ToGeoShape()
                    },
                    ConsiderObservationAccuracy = true
                }
            };

            // Act            
            var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions));
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.TotalCount.Should().Be(60,
                because: "60 observations added to Elasticsearch are located inside the circle polygon used in the filter " +
                         "when observation accuracy is considered");
        }

        [Fact]
        public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByPolygonAndConsiderDisturbanceRadius()
        {
            // Arrange
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All().HaveValuesFromPredefinedObservations()
                .TheFirst(30).HaveCoordinatesInSpan(1.00001, 1.00009, 1.00001, 1.00009, 10)
                             .With(o => o.TaxonId = 100011) // Disturbance radius: 2000m
                 .TheNext(30).HaveCoordinatesInSpan(1.00100, 1.00900, 1.00001, 1.00009, 10)
                             .With(o => o.TaxonId = 100009) // Disturbance radius: 1000m
                 .TheNext(20).HaveCoordinatesInSpan(2.00001, 2.00009, 1.00001, 1.00009, 10)
                             .With(o => o.TaxonId = 102933) // Disturbance radius: 0m
                 .TheNext(20).HaveCoordinatesInSpan(1.00001, 1.00009, 2.00001, 2.00009, 10)
                             .With(o => o.TaxonId = 100009) // Disturbance radius: 1000m
                .Build();
            var apiClient = TestFixture.CreateApiClient();
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new[] {
                        new Point(1.00005, 1.00004).ToCircle(10).ToGeoShape()
                    },
                    ConsiderDisturbanceRadius = true
                }
            };

            // Act            
            var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions));
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.TotalCount.Should().Be(60,
                because: "60 observations added to Elasticsearch are located inside the circle polygon used in the filter " +
                         "when disturbance radius is considered");
        }

        [Fact]
        public async Task ObservationsBySearchEndpoint_ReturnsExpectedObservations_WhenFilteringByBoundingBox()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All().HaveValuesFromPredefinedObservations()
                .TheFirst(60).HaveCoordinatesInSpan(1.00001, 1.00009, 1.00001, 1.00009, 10)
                .TheNext(20).HaveCoordinatesInSpan(2.00001, 2.00009, 1.00001, 1.00009, 10)
                .TheNext(20).HaveCoordinatesInSpan(1.00001, 1.00009, 2.00001, 2.00009, 10)
                .Build();
            var apiClient = TestFixture.CreateApiClient();
            await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    BoundingBox = new LatLonBoundingBoxDto
                    {
                        BottomRight = new LatLonCoordinateDto { Latitude = 1, Longitude = 1.1 },
                        TopLeft = new LatLonCoordinateDto { Latitude = 1.1, Longitude = 1 }
                    }
                }
            };

            // Act            
            var response = await apiClient.PostAsync($"/observations/search", JsonContent.Create(searchFilter, null, JsonSerializerOptions));
            var result = await response.Content.ReadFromJsonAsync<PagedResultDto<Observation>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result!.TotalCount.Should().Be(60,
                because: "60 observations added to Elasticsearch are located inside bounding box used in the filter.");
        }
    }
}
