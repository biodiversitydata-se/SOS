using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.TaxonAggregationInternalEndpoint;

[Collection(TestCollection.Name)]
public class TaxonAggregationInternalTests : ElasticsearchTestBase
{
    public TaxonAggregationInternalTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task TaxonAggregationInternalTest()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
            .All()
                .HaveValuesFromPredefinedObservations()
            .TheFirst(2)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 1, 1))
                .With(p => p.EndDate = new DateTime(2000, 1, 1))
            .TheNext(2)
                .With(p => p.TaxonId = 100013)
                .With(p => p.StartDate = new DateTime(2000, 1, 15))
                .With(p => p.EndDate = new DateTime(2000, 1, 18))
            .TheNext(2)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 1, 30))
                .With(p => p.EndDate = new DateTime(2000, 1, 30))
            .TheNext(2)
                .With(p => p.TaxonId = 100013)
                .With(p => p.StartDate = new DateTime(2000, 2, 1))
                .With(p => p.EndDate = new DateTime(2000, 2, 1))
            .TheLast(2)
                .With(p => p.TaxonId = 100012)
                .With(p => p.StartDate = new DateTime(2000, 4, 1))
                .With(p => p.EndDate = new DateTime(2000, 4, 15))
            .Build();
        var processed = await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterAggregationInternalDto
        {
            Date = new DateFilterDto
            {
                StartDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromHours(1)),
                EndDate = new DateTimeOffset(2000, 1, 31, 23, 59, 59, TimeSpan.FromHours(1)),
                DateFilterType = DateFilterTypeDto.BetweenStartDateAndEndDate
            },
            Taxon = new TaxonFilterDto
            {
                Ids = new[] { 100012, 100013 }
            }
        };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/taxonaggregation", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<TaxonAggregationItemDto>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "");
        result.Records.Select(record => (record.TaxonId, record.ObservationCount)).Should().Equal((100012, 4), (100013, 2));
        foreach (var record in result.Records)
        {
            var dates = processed.Where(observation => observation.Taxon.Id == record.TaxonId &&
                    observation.Event.StartDate >= searchFilter.Date.StartDate.Value.UtcDateTime &&
                    observation.Event.StartDate <= searchFilter.Date.EndDate.Value.UtcDateTime)
                .Select(observation => observation.Event.StartDate!.Value.ToUniversalTime()).ToList();
            record.FirstSighting!.Value.ToUniversalTime().Should().Be(dates.Min());
            record.LastSighting!.Value.ToUniversalTime().Should().Be(dates.Max());
        }
    }

    [Fact]
    public async Task TaxonAggregationInternal_DoesNotCacheSensitiveResults()
    {
        var sensitiveTaxonIds = ProtectedSpeciesHelper.SensitiveSpeciesByCategory[3].Take(2).ToArray();
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        verbatimObservations[0].TaxonId = sensitiveTaxonIds[0];
        verbatimObservations[0].ProtectedBySystem = true;
        verbatimObservations[1].TaxonId = sensitiveTaxonIds[1];
        verbatimObservations[1].ProtectedBySystem = true;

        using var apiClient = TestFixture.CreateApiClientWithUserService(
            UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3));
        var searchFilter = new SearchFilterAggregationInternalDto
        {
            ProtectionFilter = ProtectionFilterDto.Sensitive
        };

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations.Take(1));
        using var firstResponse = await apiClient.PostAsync(
            "/observations/internal/taxonaggregation",
            JsonContent.Create(searchFilter),
            TestContext.Current.CancellationToken);
        var firstResult = await firstResponse.Content.ReadFromJsonAsync<PagedResultDto<dynamic>>(
            TestContext.Current.CancellationToken);

        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        using var secondResponse = await apiClient.PostAsync(
            "/observations/internal/taxonaggregation",
            JsonContent.Create(searchFilter),
            TestContext.Current.CancellationToken);
        var secondResult = await secondResponse.Content.ReadFromJsonAsync<PagedResultDto<dynamic>>(
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        firstResult!.TotalCount.Should().Be(1);
        secondResult!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task TaxonAggregationInternal_WithDegeneratePolygon_ReturnsBadRequest()
    {
        // Arrange
        const string json = """
            {
              "geographics": {
                "geometries": [
                  {
                    "coordinates": [
                      [
                        [16.98472499942703, 62.90148608256228],
                        [16.98472499942703, 62.90148608256228]
                      ]
                    ],
                    "type": "polygon"
                  }
                ]
              }
            }
            """;
        var apiClient = TestFixture.CreateApiClient();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await apiClient.PostAsync(
            "/observations/internal/taxonaggregation?skip=0&take=100",
            content,
            TestContext.Current.CancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        responseBody.Should().Contain("Invalid geometry");
    }
}
