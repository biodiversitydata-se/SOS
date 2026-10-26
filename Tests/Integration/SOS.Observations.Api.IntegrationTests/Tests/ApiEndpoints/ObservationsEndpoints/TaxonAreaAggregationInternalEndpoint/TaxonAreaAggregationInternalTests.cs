using FluentAssertions.Execution;
using FizzWare.NBuilder;
using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using System.Text.Json;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.TaxonAreaAggregationInternalEndpoint;

[Collection(TestCollection.Name)]
public class TaxonAreaAggregationInternalTests(TestFixture fixture, ITestOutputHelper output) : ElasticsearchTestBase(fixture, output)
{
    [Fact]
    public async Task TaxonAreaAggregation_ReturnsContractData_InJsonAndDownload()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations().With(item => item.TaxonId = 103040)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var client = TestFixture.CreateApiClient();
        var cases = new[]
        {
            (CreateFile: false, SumTaxaTree: false),
            (CreateFile: true, SumTaxaTree: false),
            (CreateFile: false, SumTaxaTree: true),
            (CreateFile: true, SumTaxaTree: true)
        };

        foreach (var (createFile, sumTaxaTree) in cases)
        {
            using var response = await client.PostAsync(
                $"/Observations/Internal/TaxonAreaAggregation?areaType=County&createFile={createFile}&sumTaxaTree={sumTaxaTree}",
                JsonContent.Create(new SearchFilterAggregationInternalDto()), TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"createFile={createFile}, sumTaxaTree={sumTaxaTree}");
            using var scope = new AssertionScope($"createFile={createFile}, sumTaxaTree={sumTaxaTree}");
            var result = await response.Content.ReadFromJsonAsync<Dictionary<int, TaxonAreaAggregation>>(TestContext.Current.CancellationToken);
            result.Should().ContainKey(103040);
            result![103040].ObservationCount.Should().Be(3);
            result[103040].ObservationCountByAreaFeatureId.Should().NotBeNull();
            result[103040].ObservationCountByAreaFeatureId!.Values.Sum().Should().Be(3);
            if (sumTaxaTree) result[103040].SumObservationCount.Should().Be(3);

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            json.RootElement.GetProperty("103040").TryGetProperty(createFile ? "ObservationCount" : "observationCount", out _).Should().BeTrue();
            if (createFile) response.Content.Headers.ContentDisposition!.FileName!.Trim('"').Should().Be("taxon-area-aggregation.json");
        }
    }
}
