using FluentAssertions;
using GeoJSON.Text.Geometry;
using SOS.Api.Contracts.Workspaces;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class WorkspaceDocumentContractTests
{
    [Fact]
    public void Document_RoundTripsSearchGeometrySettingsAndApplicationState()
    {
        const string json = """
            {
              "schemaVersion": 1,
              "name": "Fåglar i området",
              "search": {
                "dataProviderIds": [1, 3],
                "dateFilter": { "mode": "CurrentYear" },
                "observationDetails": { "presence": "ObservedAndNotFound" },
                "areas": [{ "areaType": "Municipality", "featureId": "0180" }],
                "geometries": [
                  { "type": "Point", "coordinates": [18.1, 59.3] },
                  { "type": "Polygon", "coordinates": [[[18, 59], [19, 59], [19, 60], [18, 59]]] }
                ],
                "taxonSelection": {
                  "taxa": [
                    { "taxonId": 4000104, "includeChildren": true },
                    { "taxonId": 100, "exclude": true }
                  ],
                  "taxonListIds": [228],
                  "speciesGroups": [{ "taxonListId": 46, "exclude": false }],
                  "includeChildTaxaForRedlists": true,
                  "taxonCategoryIds": [17]
                },
                "excludedOccurrenceIds": ["urn:observation:1"]
              },
              "settings": { "analysisSrid": 3006, "exportSrid": 4326, "futurePreference": true },
              "applicationState": {
                "fynddata": { "geometries": [{ "selected": false, "name": "Sparad yta" }] },
                "another-app": { "columns": ["taxon", "date"] }
              },
              "futureMetadata": { "label": "Bevara detta" }
            }
            """;

        var workspace = JsonSerializer.Deserialize<SosWorkspaceDocument>(json)!;
        var restored = JsonSerializer.Deserialize<SosWorkspaceDocument>(JsonSerializer.Serialize(workspace))!;

        restored.Name.Should().Be("Fåglar i området");
        restored.SchemaVersion.Should().Be(1);
        restored.Search.DataProviderIds.Should().Equal(1, 3);
        restored.Search.DateFilter.Mode.Should().Be(WorkspaceDateFilterMode.CurrentYear);
        restored.Search.DateFilter.FilterByObservationDate.Should().BeTrue();
        restored.Search.ObservationDetails.Presence.Should().Be(WorkspacePresence.ObservedAndNotFound);
        restored.Search.Areas!.Single().FeatureId.Should().Be("0180");
        restored.Search.TaxonSelection!.Taxa.Should().HaveCount(2);
        restored.Search.TaxonSelection.Taxa!.First().IncludeChildren.Should().BeTrue();
        restored.Search.TaxonSelection.Taxa.Last().Exclude.Should().BeTrue();
        restored.Search.TaxonSelection.TaxonListIds.Should().Equal(228);
        restored.Search.TaxonSelection.SpeciesGroups!.Single().TaxonListId.Should().Be(46);
        restored.Search.TaxonSelection.TaxonCategoryIds.Should().Equal(17);
        restored.Search.TaxonSelection.IncludeChildTaxaForRedlists.Should().BeTrue();
        restored.Search.ExcludedOccurrenceIds.Should().Equal("urn:observation:1");
        restored.Search.Geometries.Should().HaveCount(2);
        restored.Search.Geometries!.First().Should().BeOfType<Point>();
        restored.Search.Geometries.Last().Should().BeOfType<Polygon>();
        var originalJson = JsonNode.Parse(json)!;
        var restoredJson = JsonNode.Parse(JsonSerializer.Serialize(restored))!;
        foreach (var name in new[] { "settings", "applicationState", "futureMetadata" })
        {
            JsonNode.DeepEquals(originalJson[name], restoredJson[name]).Should().BeTrue(name);
        }
        JsonNode.DeepEquals(originalJson["search"]!["geometries"], restoredJson["search"]!["geometries"])
            .Should().BeTrue("GeoJSON coordinates and geometry types must survive storage");
    }

    [Fact]
    public void Document_UsesStableNamesAndEnumStringsWithDifferentSerializerDefaults()
    {
        var workspace = new SosWorkspaceDocument
        {
            Name = "Saved search",
            Search = new()
            {
                DataProviderIds = [1],
                DateFilter = new() { Mode = WorkspaceDateFilterMode.CurrentYear }
            }
        };
        var defaultJson = JsonSerializer.SerializeToNode(workspace)!;
        var webJson = JsonSerializer.SerializeToNode(workspace, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var snakeCaseJson = JsonSerializer.SerializeToNode(workspace, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        })!;

        defaultJson["schemaVersion"]!.GetValue<int>().Should().Be(1);
        defaultJson["search"]!["dateFilter"]!["mode"]!.GetValue<string>().Should().Be("CurrentYear");
        defaultJson["search"]!["observationDetails"]!["presence"]!.GetValue<string>().Should().Be("ObservedOnly");
        JsonNode.DeepEquals(defaultJson, webJson).Should().BeTrue();
        JsonNode.DeepEquals(defaultJson, snakeCaseJson).Should().BeTrue();
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":\"Old BFF workspace\",\"parameters\":{}}")]
    [InlineData("{\"schemaVersion\":1}")]
    [InlineData("{\"search\":{}}")]
    public void Document_RequiresSchemaVersionAndSearch(string json)
    {
        var deserialize = () => JsonSerializer.Deserialize<SosWorkspaceDocument>(json);

        deserialize.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData("{\"unrecognizedFilter\":true}")]
    [InlineData("{\"dateFilter\":{\"unrecognizedDateRule\":true}}")]
    [InlineData("{\"taxonSelection\":{\"taxa\":[{\"taxonId\":1,\"unrecognizedTaxonRule\":true}]}}")]
    [InlineData("{\"dateFilter\":{\"mode\":\"FutureDateMode\"}}")]
    public void Document_RejectsUnrecognizedSearchSemantics(string searchJson)
    {
        var deserialize = () => JsonSerializer.Deserialize<SosWorkspaceDocument>(
            "{\"schemaVersion\":1,\"search\":" + searchJson + "}");

        deserialize.Should().Throw<JsonException>();
    }
}
