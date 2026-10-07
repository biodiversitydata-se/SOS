using FluentAssertions;
using SOS.Lib.Enums;
using SOS.Observations.Api.Services.AdaptiveSearchLocations;
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

    [Fact]
    public void Map_PreservesProjectedLocationStatusDatesAndTaxon()
    {
        var mapped = LocationObservationMapper.Map(JsonNode.Parse(Projection)!.AsObject());

        mapped.IsGeneralized.Should().BeTrue();
        mapped.Sensitive.Should().BeTrue();
        mapped.Location.LocationId.Should().Be("locality-1");
        mapped.Location.Type.Should().Be(LocationType.Point);
        mapped.Location.DecimalLatitude.Should().Be(58.38);
        mapped.Location.DecimalLongitude.Should().Be(13.85);
        mapped.Location.CoordinateUncertaintyInMeters.Should().Be(100);
        mapped.Location.Sweref99TmX.Should().Be(432100);
        mapped.Location.Sweref99TmY.Should().Be(6472000);
        mapped.Location.Locality.Should().Be("Testlokal");
        mapped.Location.Municipality.FeatureId.Should().Be("1480");
        mapped.Location.Municipality.Name.Should().Be("Göteborg");
        mapped.Occurrence.OccurrenceId.Should().Be("observation-1");
        mapped.Occurrence.IsPositiveObservation.Should().BeTrue();
        mapped.Occurrence.IsNotRediscoveredObservation.Should().BeFalse();
        mapped.Occurrence.IsNeverFoundObservation.Should().BeFalse();
        mapped.Event.StartYear.Should().Be(2020);
        mapped.Event.EndYear.Should().Be(2021);
        mapped.Taxon.Id.Should().Be(42);
        mapped.Taxon.Attributes.IsRedlisted.Should().BeTrue();
        mapped.Taxon.Attributes.IsInvasiveInSweden.Should().BeTrue();
        mapped.ArtportalenInternal.InvasiveSpeciesTreatment.Id.Should().Be(0);
    }

    [Theory]
    [InlineData("""{"location":{"locationId":"x","type":0},"sensitive":true,"event":{"startYear":2022}}""")]
    [InlineData("""{"Location":{"LocationId":"x","Type":0},"Sensitive":true,"Event":{"StartYear":2022}}""")]
    public void Map_ReadsPropertyNamesCaseInsensitivelyAndPreservesMissingValues(string json)
    {
        var mapped = LocationObservationMapper.Map(JsonNode.Parse(json)!.AsObject());

        mapped.Location.LocationId.Should().Be("x");
        mapped.Location.Type.Should().Be((LocationType)0);
        mapped.Location.DecimalLatitude.Should().BeNull();
        mapped.Location.DecimalLongitude.Should().BeNull();
        mapped.Sensitive.Should().BeTrue();
        mapped.IsGeneralized.Should().BeFalse();
        mapped.Event.StartYear.Should().Be(2022);
        mapped.Event.EndYear.Should().BeNull();
        mapped.Occurrence.Should().BeNull();
        mapped.Taxon.Should().BeNull();
        mapped.ArtportalenInternal.Should().BeNull();
    }

    [Fact]
    public void Map_PreservesExplicitNullsAndDefaultsMissingNestedFields()
    {
        var record = JsonNode.Parse("""{"location":{"locationId":"x","decimalLatitude":null},"event":null,"taxon":{"id":12},"occurrence":{}}""")!.AsObject();

        var mapped = LocationObservationMapper.Map(record);

        mapped.Location.LocationId.Should().Be("x");
        mapped.Location.DecimalLatitude.Should().BeNull();
        mapped.Event.Should().BeNull();
        mapped.Taxon.Id.Should().Be(12);
        mapped.Taxon.Attributes.IsRedlisted.Should().BeFalse();
        mapped.Taxon.Attributes.IsInvasiveInSweden.Should().BeFalse();
        mapped.Occurrence.OccurrenceId.Should().BeNull();
        mapped.Occurrence.IsPositiveObservation.Should().BeFalse();
        mapped.Occurrence.IsNotRediscoveredObservation.Should().BeFalse();
        mapped.Occurrence.IsNeverFoundObservation.Should().BeFalse();
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
