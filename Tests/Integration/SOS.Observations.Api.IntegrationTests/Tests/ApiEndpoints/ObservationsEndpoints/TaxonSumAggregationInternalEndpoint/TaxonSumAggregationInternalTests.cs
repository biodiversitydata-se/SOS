using SOS.Api.Contracts.Common.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.TaxonSumAggregationInternalEndpoint;

[Collection(TestCollection.Name)]
public class TaxonSumAggregationInternalTests : ElasticsearchTestBase
{
    [Fact]
    public async Task SortFields_AreCaseInsensitive_ButUnknownFieldsAreRejected()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .All().With(observation => observation.TaxonId = 100011)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var client = TestFixture.CreateApiClient();

        foreach (var field in new[] { "SumObservationCount", "ObservationCount", "SumProvinceCount", "ProvinceCount" })
        {
            string? expected = null;
            foreach (var spelling in new[] { field, char.ToLowerInvariant(field[0]) + field[1..], field.ToUpperInvariant() })
            {
                using var response = await client.PostAsync($"/observations/internal/taxonsumaggregation?SORTBY={spelling}&SORTORDER=DESC",
                    JsonContent.Create(new TaxonFilterDto { Ids = [100011] }), TestContext.Current.CancellationToken);
                response.StatusCode.Should().Be(HttpStatusCode.OK, $"sortBy={spelling}");
                var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
                using var document = JsonDocument.Parse(json);
                document.RootElement.GetProperty("records")[0].GetProperty("observationCount").GetInt32().Should().Be(3);
                expected ??= json;
                json.Should().Be(expected);
            }
        }

        using var invalidResponse = await client.PostAsync("/observations/internal/taxonsumaggregation?sortBy=Unknown",
            JsonContent.Create(new TaxonFilterDto { Ids = [100011] }), TestContext.Current.CancellationToken);
        invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public TaxonSumAggregationInternalTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
        TestFixture.ResetTaxonSumAggregationCache();
    }

    [Fact]
    public async Task SumObservationCountInternalTest()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange - Create verbatim observations
        //-----------------------------------------------------------------------------------------------------------

        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(10)
           .All()
               .HaveValuesFromPredefinedObservations()
           .TheFirst(2)
               .With(p => p.TaxonId = 100011)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 1, 1))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "1" })
           .TheNext(2)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 1, 15))
               .With(p => p.EndDate = new DateTime(2000, 1, 18))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "2" })
           .TheNext(2)
               .With(p => p.TaxonId = 100012)
               .With(p => p.StartDate = new DateTime(2000, 1, 30))
               .With(p => p.EndDate = new DateTime(2000, 1, 30))
               .With(p => p.Site.Province = new GeographicalArea { FeatureId = "3" })
           .TheNext(2)
               .With(p => p.TaxonId = 100016)
               .With(p => p.StartDate = new DateTime(2000, 1, 1))
               .With(p => p.EndDate = new DateTime(2000, 2, 1))
           .TheLast(2)
               .With(p => p.TaxonId = 100017)
               .With(p => p.StartDate = new DateTime(2000, 4, 1))
               .With(p => p.EndDate = new DateTime(2000, 4, 15))
           .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new TaxonFilterDto { Ids = new[] { 100011, 100012 } };

        // Act
        var response = await apiClient.PostAsync($"/observations/internal/taxonsumaggregation", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<PagedResultDto<TaxonSumAggregationItem>>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(2,
            because: "");
        result.Records.Select(item => item.TaxonId).Should().BeEquivalentTo(new[] { 100011, 100012 });
        result.Records.Single(item => item.TaxonId == 100011).ObservationCount.Should().Be(2);
        result.Records.Single(item => item.TaxonId == 100012).ObservationCount.Should().Be(4);
    }
}
