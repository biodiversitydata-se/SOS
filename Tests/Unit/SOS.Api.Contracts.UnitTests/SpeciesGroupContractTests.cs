using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SOS.Api.Contracts.Observations.Responses;
using SOS.Lib.Helpers;
using System.Text.Json;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class SpeciesGroupContractTests
{
    private static readonly ElasticsearchClient ElasticClient =
        new(ElasticSearchHelper.GetDefaultSettings(new Uri("http://localhost:9200")));

    [Theory]
    [InlineData(SpeciesGroup.All, 0)]
    [InlineData(SpeciesGroup.VascularPlants, 1)]
    [InlineData(SpeciesGroup.Mosses, 2)]
    [InlineData(SpeciesGroup.Lichens, 3)]
    [InlineData(SpeciesGroup.Fungi, 4)]
    [InlineData(SpeciesGroup.AlgaeAndMicroOrganisms, 5)]
    [InlineData(SpeciesGroup.Invertebrates, 7)]
    [InlineData(SpeciesGroup.Birds, 8)]
    [InlineData(SpeciesGroup.AmphibiansAndReptiles, 9)]
    [InlineData(SpeciesGroup.OtherVertebrates, 11)]
    [InlineData(SpeciesGroup.Bats, 12)]
    [InlineData(SpeciesGroup.Fishes, 14)]
    public void SpeciesGroup_PreservesArtportalenIdsStorageAndApiNames(SpeciesGroup group, int expectedId)
    {
        Assert.Equal(expectedId, (int)group);
        var attributes = new SOS.Lib.Models.Processed.Observation.TaxonAttributes { SpeciesGroup = group };

        using var domainJson = JsonDocument.Parse(JsonSerializer.Serialize(attributes));
        Assert.Equal(expectedId, domainJson.RootElement.GetProperty("SpeciesGroup").GetInt32());

        var bson = attributes.ToBsonDocument();
        Assert.Equal(expectedId, bson["SpeciesGroup"].AsInt32);
        Assert.Equal(group, BsonSerializer.Deserialize<SOS.Lib.Models.Processed.Observation.TaxonAttributes>(bson).SpeciesGroup);

        using var elasticJson = JsonDocument.Parse(ElasticClient.SourceSerializer.SerializeToString(attributes));
        Assert.Equal(expectedId, elasticJson.RootElement.GetProperty("speciesGroup").GetInt32());

        var dto = new TaxonAttributes { SpeciesGroup = attributes.SpeciesGroup };
        Assert.Equal(group, dto.SpeciesGroup);
        using var apiJson = JsonDocument.Parse(JsonSerializer.Serialize(dto));
        Assert.Equal(group.ToString(), apiJson.RootElement.GetProperty("SpeciesGroup").GetString());
        Assert.Equal(group, JsonSerializer.Deserialize<TaxonAttributes>(apiJson.RootElement.GetRawText())!.SpeciesGroup);
        Assert.Equal(group, JsonSerializer.Deserialize<TaxonAttributes>($"{{\"SpeciesGroup\":{expectedId}}}")!.SpeciesGroup);
    }

    [Fact]
    public void SpeciesGroup_ExposesOnlySupportedArtportalenIds()
    {
        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 11, 12, 14 },
            Enum.GetValues<SpeciesGroup>().Select(value => (int)value));
    }
}
