using FizzWare.NBuilder;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using System.Threading.Tasks;
using Xunit;
using SOS.Lib.Models.Verbatim.Artportalen;
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
                    .HaveAreaFeatureIds("X", "Y", "Z")
                .TheNext(20)
                    .HaveAreaFeatureIds("X", "Y", "Y")
                .TheNext(20)
                    .HaveAreaFeatureIds("X", "Y", "X")
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto { Areas = new [] { new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "Z" } }}
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
                    .HaveAreaFeatureIds("Px", "Cx", "Mx")
                .TheNext(30)
                    .HaveAreaFeatureIds("Px", "Cx", "My")
                .TheNext(20)
                   .HaveAreaFeatureIds("Px", "Cx", "Mz")
                .TheNext(20)
                    .HaveAreaFeatureIds("Px", "Cx", "Mv")
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto { Areas = new[] { 
                    new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "Mx" },
                    new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "My" }
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
                    .HaveAreaFeatureIds("X", "Y", "Z")
                .TheNext(20)
                    .HaveAreaFeatureIds("X", "Y", "X")
                .TheNext(20)
                     .HaveAreaFeatureIds("Y", "Y", "Z")
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterDto
            {
                Geographics = new GeographicsFilterDto { Areas = new[] { 
                    new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Province, FeatureId = "X" },
                    new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.Municipality, FeatureId = "Z" }
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

       
    }
}
