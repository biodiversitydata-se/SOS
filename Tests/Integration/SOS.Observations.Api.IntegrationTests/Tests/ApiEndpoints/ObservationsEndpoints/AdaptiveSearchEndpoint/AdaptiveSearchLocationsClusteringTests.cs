using SOS.Api.Contracts.Observations.Requests;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.TestData;
using System.Text.Json.Nodes;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

public partial class AdaptiveSearchLocationsTests
{
    [Theory]
    [InlineData(AdaptiveSearchLocationsStrategy.ObservationsThenGrid)]
    [InlineData(AdaptiveSearchLocationsStrategy.Auto)]
    public async Task PixelClusteringCutoff_ReevaluatesResolutionAcrossFullRepresentativeAndCachedResults(AdaptiveSearchLocationsStrategy strategy)
    {
        var observations = CreatePointObservations(3, 399301, TaxonIds.Sjögull, index => (15 + index * 0.0001, 59));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        using var client = TestFixture.CreateApiClient();
        var filter = CreateBoundingBoxFilter(TaxonIds.Sjögull, 14.99, 59.01, 15.01, 58.99);
        var cases = new (string Query, int Count, string Grouping)[]
        {
            ("&metersPerPixel=3", 1, "pixelCluster"),
            ("&metersPerPixel=2", 3, "singleLocation"),
            ("&metersPerPixel=1&switchToDetailClusteringAtMetersPerPixel=0", 1, "pixelCluster"),
            ("&metersPerPixel=3", 1, "pixelCluster")
        };
        for (var index = 0; index < cases.Length; index++)
        {
            var item = cases[index];
            var query = $"/observations/internal/adaptivesearchlocations?adaptiveSearchStrategy={strategy}&observationsLimit=100" +
                        "&returnInputBBox=true&clusterRadiusPx=40&minLocationCountBeforeClustering=0&detailClusterRadiusPx=0" + item.Query;
            using var response = await client.PostAsync(query, JsonContent.Create(filter), TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.GetHeaderValue("X-Cache").Should().Be(strategy == AdaptiveSearchLocationsStrategy.Auto ? "BYPASS" : index == 0 ? "MISS" : "HIT");
            response.GetHeaderValue("X-Locations-Before-Clustering").Should().Be("3");
            var features = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!["features"]!.AsArray();
            features.Should().HaveCount(item.Count);
            features.Should().OnlyContain(feature => feature!["properties"]!["groupingType"]!.GetValue<string>() == item.Grouping);
            features.Sum(feature => feature!["properties"]!["observationCount"]!.GetValue<int>()).Should().Be(3);
        }
    }

    [Theory]
    [InlineData(AdaptiveSearchLocationsStrategy.ObservationsThenGrid)]
    [InlineData(AdaptiveSearchLocationsStrategy.Auto)]
    public async Task DetailClustering_ReevaluatesZoomAndCountThresholdsForFullRepresentativeAndCachedResults(AdaptiveSearchLocationsStrategy strategy)
    {
        var observations = CreatePointObservations(3, 399401, TaxonIds.Sjögull, index => (15 + index * 0.00002, 59));
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(observations);
        // Other tests replace indexed observations while the shared host's location cache survives.
        using var factory = TestFixture.ApiFactory.WithWebHostBuilder(_ => { });
        using var client = factory.CreateClient();
        var filter = CreateBoundingBoxFilter(TaxonIds.Sjögull, 14.99, 59.01, 15.01, 58.99);
        var cases = new (string Query, int Count, string Grouping, double? Radius)[]
        {
            ("&metersPerPixel=1", 1, "pixelCluster", AdaptiveSearchLocationsDefaults.DetailClusterRadiusPx),
            ("&metersPerPixel=5.062290106003781", 1, "pixelCluster", 5.062290106003781 * AdaptiveSearchLocationsDefaults.ClusterRadiusPx),
            ("&metersPerPixel=1&detailClusterRadiusPx=0", 3, "singleLocation", null),
            ("&metersPerPixel=1&cluster=false", 3, "singleLocation", null),
            ("&metersPerPixel=3&minLocationCountBeforeClustering=3", 1, "pixelCluster", 3 * AdaptiveSearchLocationsDefaults.DetailClusterRadiusPx),
            ("&metersPerPixel=1&clusterRadiusMeters=100", 1, "meterCluster", 100),
            ("&metersPerPixel=1&detailClusterRadiusPx=500", 1, "pixelCluster", 500),
            ("&metersPerPixel=1", 1, "pixelCluster", AdaptiveSearchLocationsDefaults.DetailClusterRadiusPx)
        };
        for (var index = 0; index < cases.Length; index++)
        {
            var item = cases[index];
            var query = $"/observations/internal/adaptivesearchlocations?adaptiveSearchStrategy={strategy}&observationsLimit=100" +
                        "&returnInputBBox=true" + item.Query;
            using var response = await client.PostAsync(query, JsonContent.Create(filter), TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.GetHeaderValue("X-Cache").Should().Be(strategy == AdaptiveSearchLocationsStrategy.Auto ? "BYPASS" : index == 0 ? "MISS" : "HIT");
            response.GetHeaderValue("X-Locations-Before-Clustering").Should().Be("3");
            response.GetHeaderValue("X-Location-Presentation-Status-Approximated").Should().Be("false");
            var features = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!["features"]!.AsArray();
            features.Should().HaveCount(item.Count);
            features.Should().OnlyContain(feature => feature!["properties"]!["groupingType"]!.GetValue<string>() == item.Grouping);
            features.Sum(feature => feature!["properties"]!["observationCount"]!.GetValue<int>()).Should().Be(3);
            if (item.Radius.HasValue)
                features.Single()!["properties"]!["cluster"]!["radiusMeters"]!.GetValue<double>().Should().Be(item.Radius.Value);
        }
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("500.1")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    public async Task DetailClustering_RejectsValuesOutsideFiniteRange(string radius)
    {
        using var client = TestFixture.CreateApiClient();
        var filter = CreateBoundingBoxFilter(TaxonIds.Sjögull, 14.99, 59.01, 15.01, 58.99);
        using var response = await client.PostAsync(
            $"/observations/internal/adaptivesearchlocations?detailClusterRadiusPx={radius}",
            JsonContent.Create(filter), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Contain("detailClusterRadiusPx");
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    public async Task PixelClusteringCutoff_RejectsNegativeAndNonFiniteValues(string cutoff)
    {
        using var client = TestFixture.CreateApiClient();
        var filter = CreateBoundingBoxFilter(TaxonIds.Sjögull, 14.99, 59.01, 15.01, 58.99);
        using var response = await client.PostAsync(
            $"/observations/internal/adaptivesearchlocations?switchToDetailClusteringAtMetersPerPixel={cutoff}",
            JsonContent.Create(filter), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Contain("switchToDetailClusteringAtMetersPerPixel");
    }
}
