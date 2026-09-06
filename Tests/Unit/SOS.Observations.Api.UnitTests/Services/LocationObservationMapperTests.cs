using FluentAssertions;
using SOS.Lib.Extensions;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class LocationObservationMapperTests
{
    private const string Projection = """
        {
          "isGeneralized": true, "sensitive": true,
          "location": {
            "locationId": "locality-1", "type": 1,
            "decimalLatitude": 58.38, "decimalLongitude": 13.85,
            "coordinateUncertaintyInMeters": 100,
            "sweref99TmX": 432100, "sweref99TmY": 6472000,
            "locality": "Testlokal",
            "municipality": { "featureId": "1480", "name": "Göteborg" }
          },
          "occurrence": {
            "occurrenceId": "observation-1", "isPositiveObservation": true,
            "isNotRediscoveredObservation": false, "isNeverFoundObservation": false
          },
          "event": { "startYear": 2020, "endYear": 2021 },
          "taxon": { "id": 42, "attributes": { "isRedlisted": true, "isInvasiveInSweden": true } },
          "artportalenInternal": { "invasiveSpeciesTreatment": { "id": 0 } }
        }
        """;

    [Theory]
    [InlineData(Projection)]
    [InlineData("""{"Location":{"LocationId":"x","Type":0},"Sensitive":true,"Event":{"StartYear":2022}}""")]
    [InlineData("""{"location":{"locationId":"x","decimalLatitude":null},"event":null,"taxon":{"id":12},"occurrence":{}}""")]
    public void Map_PreservesProjectedValuesComparedWithPreviousConversion(string json)
    {
        var record = JsonNode.Parse(json)!.AsObject();
        var previous = new[] { record }.ToObservationsArray()[0];
        var mapped = LocationObservationMapper.Map(record);
        // The model initializes Created to the current time; it is not part of this projection.
        mapped.Created = previous.Created;

        JsonSerializer.Serialize(mapped).Should().Be(JsonSerializer.Serialize(previous));
    }

    [Fact]
    public void Aggregation_PreservesMixedStatusesYearsAndRepresentativeCounts()
    {
        var first = JsonNode.Parse(Projection)!.AsObject();
        var second = first.DeepClone().AsObject();
        second["sensitive"] = false;
        second["isGeneralized"] = false;
        second["occurrence"]!["isPositiveObservation"] = false;
        second["occurrence"]!["isNotRediscoveredObservation"] = true;
        second["event"]!["endYear"] = null;
        second["event"]!["startYear"] = 2024;
        second["artportalenInternal"] = null;
        second["taxon"]!["id"] = 43;

        var aggregator = new AdaptiveSearchLocationAggregator();
        var result = aggregator.CreateAggregates(new[] { first, second }).Single();
        result.TaxonIds.Should().BeEquivalentTo(new[] { 42, 43 });
        result.ObservationSummary.ObservationCount.Should().Be(2);
        result.ObservationSummary.FoundCount.Should().Be(1);
        result.ObservationSummary.NotRecoveredCount.Should().Be(1);
        result.ObservationSummary.DiffusedCount.Should().Be(1);
        result.ObservationSummary.TreatedCount.Should().Be(1);
        result.ObservationSummary.MaxPresentEndYear.Should().Be(2021);
        result.ObservationSummary.MaxNotRecoveredEndYear.Should().Be(2024);
        result.ObservationSummary.IsAllObservationsSensitive.Should().BeFalse();

        var representative = aggregator.CreateAggregates(new[] { first },
            new Dictionary<string, long> { ["locality-1"] = 25 }, true).Single();
        representative.ObservationSummary.ObservationCount.Should().Be(25);
        representative.IsSummaryApproximated.Should().BeTrue();
        representative.ObservationSummary.DebugObservations.Should().BeNull();
    }

    [Fact]
    public void Aggregation_SkipsRecordsWithoutUsableLocations()
    {
        var result = new AdaptiveSearchLocationAggregator().CreateAggregates(new[]
        {
            new JsonObject(), JsonNode.Parse("""{"location":{"locationId":" "}}""")!.AsObject()
        });
        result.Should().BeEmpty();
    }
}
