﻿using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ExportsEndpoints;

[Collection(TestCollection.Name)]
public class DownloadDwcTests : TestBase
{
    public DownloadDwcTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DownloadDwcFileEndpoint_ReturnsExpectedRows_WhenNoFilterIsUsed()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(100)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { };

        // Act
        
        var response = await apiClient.PostAsync($"/exports/download/dwc", JsonContent.Create(searchFilter));
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        contentBytes.Length.Should().BeGreaterThan(0);
        var fileEntries = DwcaHelper.ReadOccurrenceDwcFile(contentBytes);
        fileEntries.Count.Should().Be(100);
    }


}
