using FizzWare.NBuilder;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;
using System.Text;
using System.Text.Json.Nodes;
using FluentAssertions.Execution;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

[Collection(TestCollection.Name)]
public class OutputFieldsTests(TestFixture fixture, ITestOutputHelper output) : ElasticsearchTestBase(fixture, output)
{
    [Fact]
    public async Task SearchWithoutOutput_ReturnsMinimumFields_ForPublicAndInternalEndpoints()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(1)
            .All().HaveValuesFromPredefinedObservations()
            .All().WithStartEndDate("2020-05-01", "2020-05-01")
            .All().With(observation => observation.TaxonId = 100011)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var client = TestFixture.CreateApiClient();
        const string filterJson = """
            {
                "date":{"startDate":"1990-01-31T07:59:46.862Z","endDate":"2026-06-30T07:59:46.862Z"},
                "taxon":{"ids":[100011],"includeUnderlyingTaxa":false},
                "occurrenceStatus":"present"
            }
            """;

        foreach (var endpoint in new[] { "/Observations/Search", "/Observations/Internal/Search" })
        {
            using var scope = new AssertionScope(endpoint);
            using var defaultRequest = new StringContent(filterJson, Encoding.UTF8, "application/json");
            using var defaultResponse = await client.PostAsync(endpoint, defaultRequest, TestContext.Current.CancellationToken);
            defaultResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var defaultResult = await defaultResponse.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken);
            var defaultRecords = defaultResult!["records"]!.AsArray();
            defaultRecords.Should().ContainSingle();
            defaultRecords[0]!["taxon"]!["id"]!.GetValue<int>().Should().Be(100011);
            defaultRecords[0]!.AsObject().Should().NotContainKey("basisOfRecord");

            var minimumFilter = JsonNode.Parse(filterJson)!.AsObject();
            minimumFilter["output"] = new JsonObject { ["fieldSet"] = "Minimum" };
            using var minimumRequest = new StringContent(minimumFilter.ToJsonString(), Encoding.UTF8, "application/json");
            using var minimumResponse = await client.PostAsync(endpoint, minimumRequest, TestContext.Current.CancellationToken);
            minimumResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var minimumResult = await minimumResponse.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken);

            JsonNode.DeepEquals(defaultRecords, minimumResult!["records"]).Should().BeTrue(
                "omitting output should return exactly the fields selected by OutputFieldSet.Minimum");
        }
    }

    [Fact]
    public async Task OutputFields_AreCaseInsensitive_AndNoneDoesNotAddPredefinedFields()
    {
        var observations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .All().With(observation => observation.TaxonId = 100011)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var client = TestFixture.CreateApiClient();

        foreach (var endpoint in new[] { "/Observations/Search", "/Observations/Internal/Search" })
        {
            foreach (var field in new[] { "taxon.scientificName", "Taxon.ScientificName", "taxon.scientificname", "TAXON.SCIENTIFICNAME", "TaXoN.ScIeNtIfIcNaMe" })
            {
                foreach (var fieldSet in new[] { "", "\"FIELDSET\":\"nOnE\"," })
                {
                    using var request = new StringContent(
                        $$$"""{"OUTPUT":{ {{{fieldSet}}} "FIELDS":["{{{field}}}"]}}""", Encoding.UTF8, "application/json");
                    using var response = await client.PostAsync(endpoint, request, TestContext.Current.CancellationToken);

                    response.StatusCode.Should().Be(HttpStatusCode.OK, $"endpoint={endpoint}, field={field}, fieldSet={fieldSet}");
                    using var scope = new AssertionScope($"endpoint={endpoint}, field={field}, fieldSet={fieldSet}");
                    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
                    var records = json.RootElement.GetProperty("records").EnumerateArray().ToArray();
                    records.Should().HaveCount(3);
                    foreach (var record in records)
                    {
                        record.EnumerateObject().Select(property => property.Name).Should().Equal("taxon");
                        record.GetProperty("taxon").GetProperty("scientificName").GetString().Should().Be("Aquila chrysaetos");
                    }
                }
            }

            using var emptyRequest = new StringContent("""{"output":{"fieldSet":"None"}}""", Encoding.UTF8, "application/json");
            using var emptyResponse = await client.PostAsync(endpoint, emptyRequest, TestContext.Current.CancellationToken);
            emptyResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"endpoint={endpoint}, fieldSet=None, fields=empty");
            using var emptyScope = new AssertionScope($"endpoint={endpoint}, fieldSet=None, fields=empty");
            using var emptyJson = JsonDocument.Parse(await emptyResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            var emptyRecords = emptyJson.RootElement.GetProperty("records").EnumerateArray().ToArray();
            emptyRecords.Should().HaveCount(3);
            foreach (var record in emptyRecords)
            {
                record.EnumerateObject().Should().BeEmpty();
            }
        }
    }
}
