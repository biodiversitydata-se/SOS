using FluentAssertions.Execution;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Setup.Stubs;
using SOS.Observations.Api.IntegrationTests.TestData;
using System.Text.Json.Nodes;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ObservationsEndpoints.ObservationsBySearchEndpoint;

public partial class AdaptiveSearchLocationsTests
{
    [Fact]
    public async Task Auto_MatchesCompletePresentation_ForOccurrenceStatusScenariosAndHomogeneousNegativeFilters()
    {
        var verbatim = CreatePresentLocationObservations(18, 399001, TaxonIds.Skata, "Auto status scenarios", 15, 59);
        for (var i = 0; i < verbatim.Count; i++)
        {
            ConfigureLocationObservation(verbatim[i], 399001, i % 2 == 0 ? TaxonIds.Skata : TaxonIds.Talgoxe,
                "Auto status scenarios", 15, 59, notPresent: i % 3 == 1, notRecovered: i % 3 == 2, endYear: 2023 + i / 6);
        }
        var observations = ProcessFixture.ProcessObservations(verbatim);
        for (var i = 0; i < observations.Count; i++)
        {
            observations[i].ArtportalenInternal.InvasiveSpeciesTreatment = observations[i].Occurrence.InvasiveSpeciesTreatment =
                i >= 9 ? new VocabularyValue { Id = 0 } : null;
        }
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        using var client = TestFixture.CreateApiClient();
        var cases = new (OccurrenceStatusFilterValuesDto? Occurrence, SightingNotRecoveredFilterDto Recovered, SightingNotPresentFilterDto Present, bool? Treatment, int[]? TreatmentIds, bool Representative)[]
        {
            (OccurrenceStatusFilterValuesDto.Present, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.DontIncludeNotPresent, null, null, true),
            (OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.IncludeNotPresent, null, null, false),
            (OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.OnlyNotRecovered, SightingNotPresentFilterDto.IncludeNotPresent, null, null, false),
            (OccurrenceStatusFilterValuesDto.BothPresentAndAbsent, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.IncludeNotPresent, null, null, false),
            (OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.DontIncludeNotRecovered, SightingNotPresentFilterDto.IncludeNotPresent, null, null, false),
            (OccurrenceStatusFilterValuesDto.BothPresentAndAbsent, SightingNotRecoveredFilterDto.DontIncludeNotRecovered, SightingNotPresentFilterDto.IncludeNotPresent, null, null, false),
            (OccurrenceStatusFilterValuesDto.BothPresentAndAbsent, SightingNotRecoveredFilterDto.IncludeNotRecovered, SightingNotPresentFilterDto.DontIncludeNotPresent, null, null, false),
            (null, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.NoFilter, null, null, true),
            (OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.OnlyNotRecovered, SightingNotPresentFilterDto.IncludeNotPresent, false, null, true),
            (OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.DontIncludeNotRecovered, SightingNotPresentFilterDto.IncludeNotPresent, false, null, true),
            (OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.IncludeNotPresent, null, [0], true),
            (OccurrenceStatusFilterValuesDto.Absent, SightingNotRecoveredFilterDto.NoFilter, SightingNotPresentFilterDto.IncludeNotPresent, true, null, false)
        };

        foreach (var item in cases)
        foreach (var cutoff in new int?[] { null, 2024 })
        {
            using var scope = new AssertionScope($"{item}, cutoff={cutoff}");
            var filter = CreateBoundingBoxFilter(TaxonIds.Skata, 14.9, 59.1, 15.1, 58.9);
            filter.Taxon.Ids = [TaxonIds.Skata, TaxonIds.Talgoxe];
            filter.OccurrenceStatus = item.Occurrence;
            filter.NotRecoveredFilter = item.Recovered;
            filter.ExtendedFilter = new ExtendedFilterDto
            {
                NotPresentFilter = item.Present, HasInvasiveSpeciesTreatment = item.Treatment,
                InvasiveSpeciesTreatmentIds = item.TreatmentIds
            };
            var query = "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&useCache=false" +
                        (cutoff.HasValue ? $"&cutoffYear={cutoff}" : "");
            using var auto = await client.PostAsync(query + "&adaptiveSearchStrategy=Auto", JsonContent.Create(filter), TestContext.Current.CancellationToken);
            using var full = await client.PostAsync(query + "&adaptiveSearchStrategy=ObservationsThenGrid", JsonContent.Create(filter), TestContext.Current.CancellationToken);
            auto.StatusCode.Should().Be(HttpStatusCode.OK);
            full.StatusCode.Should().Be(HttpStatusCode.OK);
            auto.GetHeaderValue("X-Location-Presentation-Status-Approximated").Should().Be("false");
            auto.Headers.Contains("X-Location-Aggregation-Mode").Should().Be(item.Representative && !cutoff.HasValue);
            auto.GetHeaderValue("X-AdaptiveSearch-Strategy").Should().Be(item.Representative && !cutoff.HasValue ? "LocationsThenGrid" : "ObservationsThenGrid");
            full.GetHeaderValue("X-AdaptiveSearch-Strategy").Should().Be("ObservationsThenGrid");
            var actual = GetFeaturePropertiesByLocationId(await auto.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Values.Single();
            var expected = GetFeaturePropertiesByLocationId(await full.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Values.Single();
            GetPresentation(actual)["status"]!.ToJsonString().Should().Be(GetPresentation(expected)["status"]!.ToJsonString());
            GetPresentation(actual)["cutoffYearStatus"]?.ToJsonString().Should().Be(GetPresentation(expected)["cutoffYearStatus"]?.ToJsonString());
            actual["isPresentationStatusApproximated"]!.GetValue<bool>().Should().BeFalse();
            actual["observationCount"]!.GetValue<int>().Should().Be(expected["observationCount"]!.GetValue<int>());
            if (item.Representative && !cutoff.HasValue)
                actual["isSummaryApproximated"]!.GetValue<bool>().Should().BeTrue();
        }
    }

    [Fact]
    public async Task Auto_HandlesSensitiveAndMixedProtection_AndPreservesCutoffPresentation()
    {
        var verbatim = CreatePresentLocationObservations(6, 399101, TaxonIds.Skata, "Auto sensitive statuses", 15, 59);
        for (var i = 0; i < verbatim.Count; i++)
            ConfigureLocationObservation(verbatim[i], 399101, TaxonIds.Skata, "Auto sensitive statuses", 15, 59,
                notPresent: false, notRecovered: false, endYear: 2023 + i % 3);
        var observations = ProcessFixture.ProcessObservations(verbatim);
        for (var i = 0; i < observations.Count; i++)
        {
            observations[i].Sensitive = i >= 3;
            observations[i].Occurrence.SensitivityCategory = i >= 3 ? 3 : 1;
        }
        await ProcessFixture.AddObservationsToElasticsearchAsync(observations);
        using var client = TestFixture.CreateApiClientWithReplacedService(UserServiceStubFactory.CreateWithSightingAuthority(maxProtectionLevel: 3));

        foreach (var protection in new[] { ProtectionFilterDto.Public, ProtectionFilterDto.Sensitive, ProtectionFilterDto.BothPublicAndSensitive })
        foreach (var cutoff in new int?[] { null, 2024 })
        {
            using var scope = new AssertionScope($"protection={protection}, cutoff={cutoff}");
            var filter = CreateBoundingBoxFilter(TaxonIds.Skata, 14.9, 59.1, 15.1, 58.9);
            filter.ProtectionFilter = protection;
            var query = "/observations/internal/adaptivesearchlocations?observationsLimit=100&cluster=false&useCache=false" +
                        (cutoff.HasValue ? $"&cutoffYear={cutoff}" : "");
            using var auto = await client.PostAsync(query + "&adaptiveSearchStrategy=Auto", JsonContent.Create(filter), TestContext.Current.CancellationToken);
            using var full = await client.PostAsync(query + "&adaptiveSearchStrategy=ObservationsThenGrid", JsonContent.Create(filter), TestContext.Current.CancellationToken);
            auto.StatusCode.Should().Be(HttpStatusCode.OK);
            full.StatusCode.Should().Be(HttpStatusCode.OK);
            auto.GetHeaderValue("X-Location-Presentation-Status-Approximated").Should().Be("false");
            auto.GetHeaderValue("X-AdaptiveSearch-Strategy").Should().Be(protection != ProtectionFilterDto.BothPublicAndSensitive && !cutoff.HasValue ? "LocationsThenGrid" : "ObservationsThenGrid");
            var actual = GetFeaturePropertiesByLocationId(await auto.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Values.Single();
            var expected = GetFeaturePropertiesByLocationId(await full.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Values.Single();
            GetPresentation(actual)["status"]!.ToJsonString().Should().Be(GetPresentation(expected)["status"]!.ToJsonString());
            GetPresentation(actual)["cutoffYearStatus"]?.ToJsonString().Should().Be(GetPresentation(expected)["cutoffYearStatus"]?.ToJsonString());
            actual["isPresentationStatusApproximated"]!.GetValue<bool>().Should().BeFalse();
            actual["observationCount"]!.GetValue<int>().Should().Be(expected["observationCount"]!.GetValue<int>());
        }
    }

    [Theory]
    [InlineData("0")]
    [InlineData("10001")]
    [InlineData("null")]
    [InlineData("invalid")]
    public async Task Auto_RejectsInvalidLocationAggregationLimit(string limit)
    {
        using var client = TestFixture.CreateApiClient();
        using var response = await client.PostAsync($"/observations/internal/adaptivesearchlocations?locationAggregationLimit={limit}",
            JsonContent.Create(new SearchFilterInternalDto()), TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
