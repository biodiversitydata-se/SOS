using SOS.Api.Contracts.Common.Geo;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SOS.Lib.Gis;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Verbatim.Artportalen;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Models;

public class SharedContractModelTests
{
    [Theory]
    [InlineData(CoordinateSys.WebMercator, 3857)]
    [InlineData(CoordinateSys.Rt90_25_gon_v, 3021)]
    [InlineData(CoordinateSys.SWEREF99, 4619)]
    [InlineData(CoordinateSys.SWEREF99_TM, 3006)]
    [InlineData(CoordinateSys.WGS84, 4326)]
    [InlineData(CoordinateSys.ETRS89, 4258)]
    [InlineData(CoordinateSys.ETRS89_LAEA_Europe, 3035)]
    public void SiteCoordinateSystem_PreservesNumericBson(CoordinateSys coordinateSystem, int epsg)
    {
        var document = new BsonDocument { ["VerbatimCoordinateSystem"] = epsg };

        var site = BsonSerializer.Deserialize<Site>(document);

        Assert.Equal(coordinateSystem, site.VerbatimCoordinateSystem);
        Assert.Equal(epsg, site.ToBsonDocument()["VerbatimCoordinateSystem"].AsInt32);
    }

    [Fact]
    public void StatusCounts_PreserveJsonFieldNamesAndValues()
    {
        var summary = new LocationObservationSummary
        {
            FoundCount = 1, TreatedCount = 2, DiffusedCount = 3,
            RedlistedCount = 4, InvasiveInSwedenCount = 5
        };

        var json = JsonSerializer.Serialize(summary.ToStatusCounts(), new JsonSerializerOptions(JsonSerializerDefaults.Web));

        using var expected = JsonDocument.Parse("""{"foundCount":1,"treatedCount":2,"diffusedCount":3,"redlistedCount":4,"invasiveInSwedenCount":5}""");
        using var actual = JsonDocument.Parse(json);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement));
    }

    [Theory]
    [InlineData("Unknown", 0)]
    [InlineData("Queued", 1)]
    [InlineData("Processing", 2)]
    [InlineData("Succeeded", 3)]
    [InlineData("Failed", 4)]
    public void ExportJobStatus_PreservesStoredNamesAndNumericJson(string storedName, int value)
    {
        var document = new BsonDocument { ["_id"] = "export-1", ["Status"] = storedName };

        var job = BsonSerializer.Deserialize<ExportJobInfo>(document);

        Assert.Equal(value, (int)job.Status);
        Assert.Equal(storedName, job.ToBsonDocument()["Status"].AsString);
        Assert.Equal(value.ToString(), JsonSerializer.Serialize(job.Status));
        Assert.Equal(value.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(job.Status));
    }

    [Theory]
    [InlineData("Csv", 0)]
    [InlineData("DwC", 1)]
    [InlineData("DwCEvent", 2)]
    [InlineData("GeoJson", 3)]
    [InlineData("Excel", 4)]
    [InlineData("AooEoo", 5)]
    [InlineData("AooEooArticle17", 6)]
    public void ExportFormat_PreservesStoredNamesAndNumericJson(string storedName, int value)
    {
        var document = new BsonDocument { ["_id"] = "export-1", ["Format"] = storedName };

        var job = BsonSerializer.Deserialize<ExportJobInfo>(document);

        Assert.Equal(value, (int)job.Format);
        Assert.Equal(storedName, job.ToBsonDocument()["Format"].AsString);
        Assert.Equal(value.ToString(), JsonSerializer.Serialize(job.Format));
        Assert.Equal(value.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(job.Format));
    }

    [Theory]
    [InlineData("Minimum", 0)]
    [InlineData("Extended", 1)]
    [InlineData("AllWithValues", 2)]
    [InlineData("All", 3)]
    [InlineData("None", 4)]
    public void OutputFieldSet_PreservesStoredNames(string storedName, int value)
    {
        var document = new BsonDocument { ["_id"] = "export-1", ["OutputFieldSet"] = storedName };

        var job = BsonSerializer.Deserialize<ExportJobInfo>(document);

        Assert.Equal(value, (int)job.OutputFieldSet!.Value);
        Assert.Equal(storedName, job.ToBsonDocument()["OutputFieldSet"].AsString);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void SearchSortOrder_PreservesNumericBson(int value)
    {
        var document = new BsonDocument { ["SortBy"] = "modified", ["SortOrder"] = value };

        var sorting = BsonSerializer.Deserialize<SortOrderFilter>(document);

        Assert.Equal(value, (int)sorting.SortOrder);
        Assert.Equal(value, sorting.ToBsonDocument()["SortOrder"].AsInt32);
    }
}
