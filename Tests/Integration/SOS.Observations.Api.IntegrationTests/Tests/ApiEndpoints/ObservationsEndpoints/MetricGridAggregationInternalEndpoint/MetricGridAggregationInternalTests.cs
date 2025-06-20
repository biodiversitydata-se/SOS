﻿using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using MongoDB.Driver.GeoJsonObjectModel;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.MetricGridAggregationInternalEndpoint;

[Collection(TestCollection.Name)]
public class MetricGridAggregationInternalTests : TestBase
{
    public MetricGridAggregationInternalTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task SumObservationCountInternalTest()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(20)
                .With(p => p.TaxonId = 100012)
                .With(p => p.Site.County = new GeographicalArea { FeatureId = "6" })
                .With(p => p.Site.XCoord = 1599118)
                .With(p => p.Site.YCoord = 7940868)
                .With(p => p.Site.Point = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(14.36512140510641, 57.87386951383448)))
            .TheNext(20)
                .With(p => p.TaxonId = 100013)
                .With(p => p.Site.County = new GeographicalArea { FeatureId = "6" })
                .With(p => p.Site.XCoord = 1563368)
                .With(p => p.Site.YCoord = 7793523)
                .With(p => p.Site.Point = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(14.043973691033681, 57.163074572716816)))
            .TheNext(20)
                .With(p => p.TaxonId = 100012)
                .With(p => p.Site.County = new GeographicalArea { FeatureId = "6" })
                .With(p => p.Site.XCoord = 1596541)
                .With(p => p.Site.YCoord = 7941260)
                .With(p => p.Site.Point = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(14.341971820234647, 57.87574209015208)))
            .TheNext(20)
                .With(p => p.TaxonId = 100012)
                .With(p => p.Site.County = new GeographicalArea { FeatureId = "7" })
                .With(p => p.Site.XCoord = 1535521)
                .With(p => p.Site.YCoord = 7764346)
                .With(p => p.Site.Point = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(13.793819833864916, 57.02067677016201)))
            .TheLast(20)
                .With(p => p.TaxonId = 100011)
                .With(p => p.Site.County = new GeographicalArea { FeatureId = "6" })
                .With(p => p.Site.XCoord = 1599118)
                .With(p => p.Site.YCoord = 7940868)
                .With(p => p.Site.Point = new GeoJsonPoint<GeoJson2DCoordinates>(new GeoJson2DCoordinates(14.36512140510641, 57.87386951383448)))
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterAggregationInternalDto
        {
            Geographics = new GeographicsFilterDto
            {
                Areas = new[] { new AreaFilterDto { AreaType = AreaTypeDto.County, FeatureId = "6" } }
            },
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100012, 100013 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/metricgridaggregation", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<GeoGridMetricResultDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.GridCells.Sum(gc => gc.ObservationsCount).Should().Be(60, because: "");
    }
}