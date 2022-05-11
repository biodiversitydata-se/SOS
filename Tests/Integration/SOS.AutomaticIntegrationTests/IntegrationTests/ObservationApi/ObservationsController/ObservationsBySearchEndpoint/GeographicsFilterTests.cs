using FizzWare.NBuilder;
using FluentAssertions;
using NetTopologySuite.Geometries;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.ObservationsBySearchEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class GeographicsFilterTests
    {
        private readonly IntegrationTestFixture _fixture;

        public GeographicsFilterTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestSigleCountyMatchFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            

            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .HaveAreaFeatureIds("P1", "C1", "M1")
                .TheNext(20)
                    .HaveAreaFeatureIds("P1", "C1", "M2")
                .TheNext(20)
                    .HaveAreaFeatureIds("P1", "C1", "M3")
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto { Areas = new [] { new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "M1" } }}
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestMultipleCountiesMatchFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .HaveAreaFeatureIds("P1", "C1", "M1")
                .TheNext(30)
                    .HaveAreaFeatureIds("P1", "C1", "M2")
                .TheNext(20)
                   .HaveAreaFeatureIds("P1", "C1", "M3")
                .TheNext(20)
                    .HaveAreaFeatureIds("P1", "C1", "M4")
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto { Areas = new[] { 
                    new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "M1" },
                    new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "M2" }
                } }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TesCountyAndProvinceMatchFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .HaveAreaFeatureIds("P1", "C1", "M1")
                .TheNext(20)
                    .HaveAreaFeatureIds("P1", "C1", "M2")
                .TheNext(20)
                     .HaveAreaFeatureIds("P1", "C1", "M3")
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto { Areas = new[] { 
                    new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Province, FeatureId = "P1" },
                    new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "M1" }
                } }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestPolygonMatchFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .HaveCoordinates(1.00001, 1.00001, 10)
                .TheNext(30)
                    .HaveCoordinates(1.00009, 1.00009, 10)
                .TheNext(20)
                    .HaveCoordinates(2.00001, 1.00001, 10)
                .TheNext(20)
                     .HaveCoordinates(1.00001, 2.00001, 10)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new []
                    {
                        (new Point(1.00005, 1.00004)).ToCircle(10).ToGeoShape()
                    }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestPolygonMaxAccuracyMatchFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(60)
                    .HaveCoordinates(1.00001, 1.00001, 10)
                .TheNext(20)
                    .HaveCoordinates(1.00001, 1.00001, 20)
                .TheNext(20)
                     .HaveCoordinates(1.00001, 2.00001, 10)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new[]
                    {
                        (new Point(1.00005, 1.00004)).ToCircle(10).ToGeoShape()
                    },
                    MaxAccuracy = 15
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestPolygonConsiderObservationAccuracyMatchFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .HaveCoordinates(1.00001, 1.00001, 10)
                .TheNext(30)
                    .HaveCoordinates(1.0001, 1.0009, 100)
                .TheNext(20)
                    .HaveCoordinates(2.00001, 1.00001, 10)
                .TheNext(20)
                     .HaveCoordinates(1.00001, 2.00001, 10)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new[]
                    {
                        (new Point(1.00005, 1.00004)).ToCircle(10).ToGeoShape()
                    },
                    ConsiderObservationAccuracy = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestPolygonConsiderDisturbanceRadiusMatchFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .HaveCoordinates(1.00001, 1.00001, 10)
                    .HaveTaxonId(100011)
                .TheNext(30)
                    .HaveCoordinates(1.001, 1.001, 10)
                     .HaveTaxonId(100009)
                .TheNext(20)
                    .HaveCoordinates(2.00001, 1.00001, 10)
                    .HaveTaxonId(102933)
                .TheNext(20)
                     .HaveCoordinates(1.00001, 2.00001, 10)
                     .HaveTaxonId(100009)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto
                {
                    Geometries = new[]
                    {
                        (new Point(1.00005, 1.00004)).ToCircle(10).ToGeoShape()
                    },
                    ConsiderDisturbanceRadius = true
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }

        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task TestBoundingBoxMatchFilter()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                .HaveValuesFromPredefinedObservations()
                .TheFirst(30)
                    .HaveCoordinates(1.00001, 1.00001, 10)
                .TheNext(30)
                    .HaveCoordinates(1.00009, 1.00009, 10)
                .TheNext(20)
                    .HaveCoordinates(2.00001, 1.00001, 10)
                .TheNext(20)
                     .HaveCoordinates(1.00001, 2.00001, 10)
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

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

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.ObservationsBySearch(
                null,
                null,
                searchFilter,
                0,
                100);
            var result = response.GetResult<PagedResultDto<Observation>>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(60);
        }
    }
}
