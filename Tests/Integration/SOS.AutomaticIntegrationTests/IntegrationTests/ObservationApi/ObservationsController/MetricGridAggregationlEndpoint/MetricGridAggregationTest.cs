﻿using FizzWare.NBuilder;
using FluentAssertions;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using SOS.AutomaticIntegrationTests.TestFixtures;
using SOS.AutomaticIntegrationTests.TestDataBuilder;
using SOS.AutomaticIntegrationTests.Extensions;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.Dtos;

namespace SOS.AutomaticIntegrationTests.IntegrationTests.ObservationApi.ObservationsController.MetricGridAggregationlEndpoint
{
    [Collection(Constants.IntegrationTestsCollectionName)]
    public class MetricGridAggregationTest
    {
        private readonly IntegrationTestFixture _fixture;

        public MetricGridAggregationTest(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }
        [Fact]
        [Trait("Category", "AutomaticIntegrationTest")]
        public async Task SumObservationCountInternalTest()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange - Create verbatim observations
            //-----------------------------------------------------------------------------------------------------------
            
            var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
                .All()
                    .HaveValuesFromPredefinedObservations()
                    //.HaveRandomValues()
                .TheFirst(20)
                    .With(p => p.TaxonId = 100012)
                    .With(p => p.Site.County = new GeographicalArea { FeatureId = "6" })
                    .With(p => p.Site.XCoord = 1599118)
                    .With(p => p.Site.YCoord = 7940868)
                    .With(p => p.Site.Point = new Lib.Models.Shared.GeoJsonGeometry { Coordinates = new System.Collections.ArrayList { 14.36512140510641, 57.87386951383448 }, Type = "Point" })
                .TheNext(20)
                    .With(p => p.TaxonId = 100013)
                    .With(p => p.Site.County = new GeographicalArea { FeatureId = "6" })
                    .With(p => p.Site.XCoord = 1563368)
                    .With(p => p.Site.YCoord = 7793523)
                    .With(p => p.Site.Point = new Lib.Models.Shared.GeoJsonGeometry { Coordinates = new System.Collections.ArrayList { 14.043973691033681, 57.163074572716816 }, Type = "Point" })
                .TheNext(20)
                    .With(p => p.TaxonId = 100012)
                    .With(p => p.Site.County = new GeographicalArea { FeatureId = "6" })
                    .With(p => p.Site.XCoord = 1596541)
                    .With(p => p.Site.YCoord = 7941260)
                    .With(p => p.Site.Point = new Lib.Models.Shared.GeoJsonGeometry { Coordinates = new System.Collections.ArrayList { 14.341971820234647, 57.87574209015208 }, Type = "Point" })
                .TheNext(20)
                    .With(p => p.TaxonId = 100012)
                    .With(p => p.Site.County = new GeographicalArea { FeatureId = "7" })
                    .With(p => p.Site.XCoord = 1535521)
                    .With(p => p.Site.YCoord = 7764346)
                    .With(p => p.Site.Point = new Lib.Models.Shared.GeoJsonGeometry { Coordinates = new System.Collections.ArrayList { 13.793819833864916, 57.02067677016201 }, Type = "Point" })
                .TheLast(20)
                    .With(p => p.TaxonId = 100011)
                    .With(p => p.Site.County = new GeographicalArea { FeatureId = "6" })
                    .With(p => p.Site.XCoord = 1599118)
                    .With(p => p.Site.YCoord = 7940868)
                    .With(p => p.Site.Point = new Lib.Models.Shared.GeoJsonGeometry { Coordinates = new System.Collections.ArrayList { 14.36512140510641, 57.87386951383448 }, Type = "Point" })
                .Build();

            await _fixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);

            var searchFilter = new SearchFilterAggregationDto
            {
                Geographics = new GeographicsFilterDto { 
                   Areas = new [] { new AreaFilterDto { AreaType = Observations.Api.Dtos.Enum.AreaTypeDto.County, FeatureId = "6" } }
                },
                Taxon = new TaxonFilterDto
                {
                    Ids = new[] { 100012, 100013 }
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act - Get observation by occurrenceId
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.ObservationsController.MetricGridAggregationAsync(null, null, searchFilter);
            var result = response.GetResult<GeoGridMetricResultDto>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------            
            result.GridCells.Sum(gc => gc.ObservationsCount).Should().Be(60);
        }
    }
}