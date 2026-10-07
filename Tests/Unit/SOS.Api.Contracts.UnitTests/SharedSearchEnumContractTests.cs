using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Common.Geo;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class SharedSearchEnumContractTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Theory]
    [InlineData(CoordinateSys.WebMercator, 3857, "WebMercator")]
    [InlineData(CoordinateSys.Rt90_25_gon_v, 3021, "Rt90_25_gon_v")]
    [InlineData(CoordinateSys.SWEREF99, 4619, "SWEREF99")]
    [InlineData(CoordinateSys.SWEREF99_TM, 3006, "SWEREF99_TM")]
    [InlineData(CoordinateSys.WGS84, 4326, "WGS84")]
    [InlineData(CoordinateSys.ETRS89, 4258, "ETRS89")]
    [InlineData(CoordinateSys.ETRS89_LAEA_Europe, 3035, "ETRS89_LAEA_Europe")]
    public void CoordinateSys_PreservesValuesAndSerialization(CoordinateSys value, int epsg, string name)
    {
        Assert.Equal(epsg, (int)value);
        Assert.Equal(epsg.ToString(), JsonSerializer.Serialize(value));
        Assert.Equal($"\"{name}\"", JsonSerializer.Serialize(value, Options));
        Assert.Equal(value, JsonSerializer.Deserialize<CoordinateSys>($"\"{name}\"", Options));
        Assert.Equal(value, JsonSerializer.Deserialize<CoordinateSys>(epsg.ToString(), Options));
    }

    [Theory]
    [InlineData("SWEREF99_TM", 3006)]
    [InlineData("ETRS89", 3035)]
    [InlineData("ETRS89_LAEA_Europe", 3035)]
    public void MetricCoordinateSys_AcceptsExistingNamesAndEpsgNumbers(string name, int epsg)
    {
        var value = JsonSerializer.Deserialize<MetricCoordinateSys>($"\"{name}\"", Options);

        Assert.Equal(epsg, (int)value);
        Assert.Equal(value, JsonSerializer.Deserialize<MetricCoordinateSys>(epsg.ToString(), Options));
        Assert.Equal(epsg.ToString(), JsonSerializer.Serialize(value));
        Assert.Equal(value, JsonSerializer.Deserialize<MetricCoordinateSys>(JsonSerializer.Serialize(value, Options), Options));
        Assert.True(Enum.IsDefined((CoordinateSys)value));
    }

    [Theory]
    [InlineData(TaxonomyType.Dyntaxa, 0, "Dyntaxa")]
    [InlineData(TaxonomyType.Artportalen, 1, "Artportalen")]
    public void TaxonomyType_PreservesValuesAndStringSerialization(TaxonomyType value, int number, string name)
    {
        Assert.Equal(number, (int)value);
        Assert.Equal(name, value.ToString());
        Assert.Equal($"\"{name}\"", JsonSerializer.Serialize(value, Options));
        Assert.Equal(value, JsonSerializer.Deserialize<TaxonomyType>($"\"{name}\"", Options));
        Assert.Equal(value, JsonSerializer.Deserialize<TaxonomyType>(number.ToString(), Options));
    }

    [Theory]
    [InlineData(AggregationType.SightingsPerWeek, 0, "SightingsPerWeek")]
    [InlineData(AggregationType.SightingsPerYear, 1, "SightingsPerYear")]
    [InlineData(AggregationType.QuantityPerWeek, 2, "QuantityPerWeek")]
    [InlineData(AggregationType.QuantityPerYear, 3, "QuantityPerYear")]
    [InlineData(AggregationType.SpeciesSightingsList, 4, "SpeciesSightingsList")]
    [InlineData(AggregationType.SpeciesSightingsListTaxonCount, 5, "SpeciesSightingsListTaxonCount")]
    [InlineData(AggregationType.SightingsPerWeek48, 6, "SightingsPerWeek48")]
    public void AggregationType_PreservesValuesAndStringSerialization(AggregationType value, int number, string name)
    {
        Assert.Equal(number, (int)value);
        Assert.Equal(name, value.ToString());
        Assert.Equal($"\"{name}\"", JsonSerializer.Serialize(value, Options));
        Assert.Equal(value, JsonSerializer.Deserialize<AggregationType>($"\"{name}\"", Options));
        Assert.Equal(value, JsonSerializer.Deserialize<AggregationType>(number.ToString(), Options));
    }
}
