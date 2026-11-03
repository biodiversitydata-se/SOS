using Moq;
using NetTopologySuite.Geometries;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Enums;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.TestData;
using SOS.Observations.Api.Services.AdaptiveSearch;
using System.Text.Json.Nodes;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

public partial class AdaptiveSearchLocationsTests
{
    [Theory]
    [InlineData(AdaptiveSearchLocationsStrategy.Auto)]
    [InlineData(AdaptiveSearchLocationsStrategy.ObservationsThenGrid)]
    public async Task PolygonClustering_ReevaluatesRenderingAcrossModesAndCachedResolutions(AdaptiveSearchLocationsStrategy strategy)
    {
        const int siteId = 399201;
        var verbatim = CreatePointObservations(4, siteId, TaxonIds.Skata, i => (15 + Math.Max(0, i - 1) * 0.0001, 59));
        verbatim[0].Site.HasGeometry = true;
        verbatim[1].Site.HasGeometry = true;
        verbatim[1].Site.Id = siteId;
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatim);
        var locationId = CreateArtportalenSiteLocationId(siteId);
        var polygons = new Dictionary<string, Geometry>
        {
            [locationId] = new GeometryFactory().ToGeometry(new Envelope(14.9995, 15.0005, 58.9995, 59.0005))
        };
        var lookup = new Mock<ILocationGeometryLookupService>(MockBehavior.Strict);
        lookup.Setup(service => service.GetPolygonLocationGeometriesAsync(It.IsAny<IEnumerable<(LocationType? Type, IEnumerable<string> LocationIds)>>()))
            .ReturnsAsync(polygons);
        using var client = TestFixture.ApiFactory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Replace(ServiceDescriptor.Singleton(lookup.Object)))).CreateClient();
        var filter = CreateBoundingBoxFilter(TaxonIds.Skata, 14.99, 59.01, 15.01, 58.99);
        var cases = new (CreatePolygonsMode Mode, int? Resolution, bool Separate)[]
        {
            (CreatePolygonsMode.No, 1, false),
            (CreatePolygonsMode.WhenNeeded, 1, true),
            (CreatePolygonsMode.WhenNeeded, 100, false),
            (CreatePolygonsMode.Yes, 100, false),
            (CreatePolygonsMode.Yes, 1, true),
            (CreatePolygonsMode.Yes, null, true)
        };

        for (var index = 0; index < cases.Length; index++)
        {
            var (mode, resolution, separate) = cases[index];
            var query = $"/observations/internal/adaptivesearchlocations?adaptiveSearchStrategy={strategy}&observationsLimit=100" +
                        $"&cluster=true&clusterRadiusMeters=100&clusterRadiusPx=40&minLocationCountBeforeClustering=0&switchToDetailClusteringAtMetersPerPixel=0" +
                        $"&createPolygons={mode}&returnInputBBox=true";
            if (resolution.HasValue) query += $"&metersPerPixel={resolution}";
            using var response = await client.PostAsync(query, JsonContent.Create(filter), TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.GetHeaderValue("X-Observations-TotalCount").Should().Be("4");
            response.GetHeaderValue("X-Locations-Before-Clustering").Should().Be("3");
            response.GetHeaderValue("X-Feature-Count").Should().Be(separate ? "2" : "1");
            response.GetHeaderValue("X-Location-Presentation-Status-Approximated").Should().Be("false");
            response.GetHeaderValue("X-Cache").Should().Be(strategy == AdaptiveSearchLocationsStrategy.Auto ? "BYPASS" : index == 0 ? "MISS" : "HIT");
            var features = JsonNode.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))!["features"]!.AsArray();
            features.Sum(feature => feature!["properties"]!["observationCount"]!.GetValue<int>()).Should().Be(4);
            if (separate)
            {
                var polygon = features.Single(feature => feature!["geometry"]!["type"]!.GetValue<string>() == "Polygon")!;
                polygon["properties"]!["groupingType"]!.GetValue<string>().Should().Be("singleLocation");
                polygon["properties"]!["observationCount"]!.GetValue<int>().Should().Be(2);
                polygon["properties"]!["isSummaryApproximated"]!.GetValue<bool>().Should().Be(strategy == AdaptiveSearchLocationsStrategy.Auto);
                polygon["properties"]!["locations"]!["ids"]!.AsArray().Select(id => id!.GetValue<string>()).Should().Equal(locationId);
            }
            else
            {
                features.Should().ContainSingle();
                features[0]!["geometry"]!["type"]!.GetValue<string>().Should().Be("Point");
            }
        }
        // Geometry is fetched once for each mode that needs it, including cache hits, never for No.
        lookup.Verify(service => service.GetPolygonLocationGeometriesAsync(It.IsAny<IEnumerable<(LocationType? Type, IEnumerable<string> LocationIds)>>()), Times.Exactly(5));
    }
}
