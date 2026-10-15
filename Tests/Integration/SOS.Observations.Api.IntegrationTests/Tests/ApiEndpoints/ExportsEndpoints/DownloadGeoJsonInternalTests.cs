using SOS.Api.Contracts.Observations.Requests;
using FizzWare.NBuilder;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using FluentAssertions.Execution;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Observations.Api.IntegrationTests.Helpers;
using SOS.Observations.Api.IntegrationTests.Setup;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData.TestDataBuilder;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ExportsEndpoints;

[Collection(TestCollection.Name)]
public class DownloadGeoJsonInternalTests : ElasticsearchTestBase
{
    public DownloadGeoJsonInternalTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsExpectedRows_WhenNoFilterWasUsed()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = SOS.Api.Contracts.Observations.Requests.OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(3, because: "3 observations were added and no filter was used.");
    }

    [Fact]
    public async Task DownloadGeoJson_PreservesInstantsAndSwedishOffsets_InFlatAndHierarchicalProperties()
    {
        var verbatim = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(2)
            .All().HaveValuesFromPredefinedObservations().Build();
        var observations = ProcessFixture.ProcessObservations(verbatim);
        var timestamps = new[]
        {
            new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc)
        };
        for (var i = 0; i < observations.Count; i++)
        {
            observations[i].Event = new Event(timestamps[i]);
            observations[i].Created = timestamps[i];
            observations[i].Modified = timestamps[i];
            observations[i].Occurrence.ReportedDate = timestamps[i];
        }
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        using var client = TestFixture.CreateApiClient();
        var filter = new SearchFilterInternalDto
        {
            Output = new OutputFilterExtendedDto
            {
                FieldSet = OutputFieldSet.Minimum,
                Fields = ["modified", "occurrence.reportedDate"]
            }
        };
        var fields = new[] { "modified", "event.startDate", "event.endDate", "occurrence.reportedDate" };

        foreach (var flat in new[] { true, false })
        {
            using var scope = new AssertionScope($"flat={flat}");
            using var response = await client.PostAsync(
                $"/exports/internal/download/geojson?gzip=false&flat={flat}&propertyLabelType=PropertyPath",
                JsonContent.Create(filter), TestContext.Current.CancellationToken);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            using var json = JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
            var features = json.RootElement.GetProperty("features").EnumerateArray().ToArray();
            features.Should().HaveCount(2);
            foreach (var feature in features)
            {
                var properties = feature.GetProperty("properties");
                foreach (var field in fields)
                {
                    var value = properties;
                    foreach (var part in flat ? new[] { field } : field.Split('.'))
                    {
                        Assert.Contains(value.EnumerateObject(), p => p.Name.Equals(part, StringComparison.OrdinalIgnoreCase));
                        value = value.EnumerateObject().Single(p => p.Name.Equals(part, StringComparison.OrdinalIgnoreCase)).Value;
                    }
                    var timestamp = value.GetDateTimeOffset();
                    timestamps.Should().Contain(timestamp.UtcDateTime);
                    timestamp.Offset.Should().Be(TimeSpan.FromHours(timestamp.Month == 1 ? 1 : 2));
                    value.GetString().Should().EndWith(timestamp.Month == 1 ? "+01:00" : "+02:00");
                }
            }
        }
    }

    [Fact]
    public async Task DownloadGeoJsonFileEndpoint_ReturnsHierarchicalJsonProperties_WhenFlatIsFalse()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(3)
            .All().HaveValuesFromPredefinedObservations()
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var apiClient = TestFixture.CreateApiClient();
        var searchFilter = new SearchFilterDto { Output = new OutputFilterDto { FieldSet = SOS.Api.Contracts.Observations.Requests.OutputFieldSet.Minimum } };

        // Act
        var response = await apiClient.PostAsync($"/exports/internal/download/geojson?gzip=false&flat=false&excludeNullValues=true", JsonContent.Create(searchFilter), TestContext.Current.CancellationToken);
        byte[] contentBytes = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fileEntries = GeoJsonHelper.ReadGeoJsonFile(contentBytes);
        fileEntries.Count.Should().Be(3, because: "3 observations were added and no filter was used.");
    }

    [Fact]
    public async Task DownloadGeoJsonInternal_AppliesProtectionFilterAndClearance()
    {
        // Arrange
        var verbatimObservations = Builder<ArtportalenObservationVerbatim>.CreateListOfSize(7)
            .All().HaveValuesFromPredefinedObservations()
            .TheFirst(3).HaveTaxonSensitivityCategory(3)
             .TheNext(2).HaveTaxonSensitivityCategory(4)
            .Build();
        await ProcessFixture.ProcessAndAddObservationsToElasticSearch(verbatimObservations);
        var userServiceStub = UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3);
        using var apiClient = TestFixture.CreateApiClientWithUserService(userServiceStub);
        var sensitiveIds = verbatimObservations.Take(3)
            .Select(observation => $"urn:lsid:artportalen.se:sighting:{observation.SightingId}").ToArray();
        var publicIds = verbatimObservations.Skip(5)
            .Select(observation => $"urn:lsid:artportalen.se:sighting:{observation.SightingId}").ToArray();
        var cases = new[]
        {
            (Protection: ProtectionFilterDto.Public, ExpectedIds: publicIds),
            (Protection: ProtectionFilterDto.Sensitive, ExpectedIds: sensitiveIds),
            (Protection: ProtectionFilterDto.BothPublicAndSensitive, ExpectedIds: publicIds.Concat(sensitiveIds).ToArray())
        };

        foreach (var testCase in cases)
        {
            using var scope = new AssertionScope($"ProtectionFilter={testCase.Protection}");
            var filter = new SearchFilterInternalDto
            {
                ProtectionFilter = testCase.Protection,
                OccurrenceStatus = OccurrenceStatusFilterValuesDto.BothPresentAndAbsent
            };
            using var response = await apiClient.PostAsync(
                "/exports/internal/download/geojson?gzip=false&flat=true&propertyLabelType=PropertyPath&sensitiveObservations=true",
                JsonContent.Create(filter), TestContext.Current.CancellationToken);
            using var json = JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken));
            var occurrenceIds = json.RootElement.GetProperty("features").EnumerateArray()
                .Select(feature => feature.GetProperty("properties").EnumerateObject()
                    .Single(property => property.Name.Equals("occurrence.occurrenceId", StringComparison.OrdinalIgnoreCase))
                    .Value.GetString());

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            occurrenceIds.Should().BeEquivalentTo(testCase.ExpectedIds,
                "only public observations and protected observations within the user's clearance may be exported");
        }
    }

}
