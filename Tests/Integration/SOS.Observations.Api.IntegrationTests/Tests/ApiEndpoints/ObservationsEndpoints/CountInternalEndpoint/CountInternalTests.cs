﻿using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.CountInternalEndpoint;

[Collection(TestCollection.Name)]
public class CountInternalTests : TestBase
{
    public CountInternalTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
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
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 1))
            .TheNext(20)
                .With(p => p.TaxonId = 100013)
                .With(p => p.StartDate = new DateTime(2000, 1, 15))
                .With(p => p.EndDate = new DateTime(2000, 1, 18))
            .TheNext(20)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 1, 30))
                .With(p => p.EndDate = new DateTime(2000, 1, 30))
            .TheNext(20)
                .With(p => p.TaxonId = 100011)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 2, 1))
            .TheLast(20)
                .With(p => p.TaxonId = 100011)
                .With(p => p.StartDate = new DateTime(2000, 4, 1))
                .With(p => p.EndDate = new DateTime(2000, 4, 15))
            .Build();

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterInternalBaseDto
        {
            Date = new DateFilterDto
            {
                StartDate = DateTime.Parse("2000-01-01T00:00:00"),
                EndDate = DateTime.Parse("2000-01-31T23:59:59"),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100012, 100013 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/count", JsonContent.Create(searchFilter));
        var result = await response.Content.ReadFromJsonAsync<int>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(60,
            because: "");
    }
}