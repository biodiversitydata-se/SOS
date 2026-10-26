﻿using SOS.Api.Contracts.Locations.Responses;
using FizzWare.NBuilder;
using NetTopologySuite.IO;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.LocationsEndpoints;

/// <summary>
/// Integration tests for GetLocationsByIds.
/// </summary>
[Collection(TestCollection.Name)]
public class GetLocationsByIds : ElasticsearchTestBase
{
    public GetLocationsByIds(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }   

    [Fact]
    public async Task GetLocationsByIds_PreservesLocationFieldsPolygonHolesAndNullGeometries()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(1).With(m => m.Site.Id = 4060923)
            .TheLast(1).With(m => m.Site.Id = 4060924)
            .Build();
        var processed = ProcessFixture.ProcessObservations(verbatimObservations);
        processed[0].Location.PointWithBuffer = new WKTReader().Read(
            "POLYGON ((14 58, 15 58, 15 59, 14 59, 14 58), (14.2 58.2, 14.2 58.8, 14.8 58.8, 14.8 58.2, 14.2 58.2))");
        processed[0].Location.PointWithDisturbanceBuffer = new WKTReader().Read(
            "MULTIPOLYGON (((13 57, 16 57, 16 60, 13 57)))");
        processed[1].Location.Point = null!;
        processed[1].Location.PointWithBuffer = null!;
        processed[1].Location.PointWithDisturbanceBuffer = null!;
        await ProcessFixture.AddObservationsToElasticsearchAsync(processed);
        var apiClient = TestFixture.CreateApiClient();
        var locationIds = processed.Select(observation => observation.Location.LocationId).ToList();

        // Act
        var response = await apiClient.PostAsync($"/Locations", JsonContent.Create(locationIds), TestContext.Current.CancellationToken);        
        var result = await response.Content.ReadFromJsonAsync<List<LocationDto>>(JsonSerializerOptions, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeEquivalentTo(processed.Select(observation => observation.Location.ToLocationDto()));
    }
}
