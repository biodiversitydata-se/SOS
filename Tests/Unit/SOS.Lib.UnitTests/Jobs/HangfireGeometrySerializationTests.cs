using GeoJSON.Text.Geometry;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Newtonsoft.Json;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Extensions;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Search.Filters;
using System.Text.Json.Nodes;
using Xunit;
using TextJsonSerializer = System.Text.Json.JsonSerializer;

namespace SOS.Lib.UnitTests.Jobs;

// Hangfire stores its serializer settings globally. Keep these tests isolated from other job tests.
[CollectionDefinition("Hangfire serialization", DisableParallelization = true)]
public class HangfireSerializationCollection;

[Collection("Hangfire serialization")]
public class HangfireGeometrySerializationTests : IDisposable
{
    public HangfireGeometrySerializationTests()
    {
        GlobalConfiguration.Configuration.UseSosHangfireSerializerSettings();
    }

    public void Dispose()
    {
        GlobalConfiguration.Configuration.UseSerializerSettings(null);
    }

    [Theory]
    [InlineData("""{"type":"Point","coordinates":[16,62,10]}""")]
    [InlineData("""{"type":"MultiPoint","coordinates":[[16,62],[17,63]]}""")]
    [InlineData("""{"type":"LineString","coordinates":[[16,62],[17,63]]}""")]
    [InlineData("""{"type":"MultiLineString","coordinates":[[[16,62],[17,63]],[[18,64],[19,65]]]}""")]
    [InlineData("""{"type":"Polygon","coordinates":[[[16,62,10],[17,62,20],[17,63,30],[16,62,10]],[[16.1,62.1,40],[16.2,62.1,50],[16.2,62.2,60],[16.1,62.1,40]]]}""")]
    [InlineData("""{"type":"MultiPolygon","coordinates":[[[[16,62],[17,62],[17,63],[16,62]]],[[[18,64],[19,64],[19,65],[18,64]]]]}""")]
    [InlineData("""{"type":"GeometryCollection","geometries":[{"type":"Point","coordinates":[16,62,10]},{"type":"LineString","coordinates":[[16,62],[17,63]]}]}""")]
    public void ExportJob_RoundTripsPublicAndInternalFiltersWithGeometries(string geometryJson)
    {
        foreach (var filter in new SearchFilterBaseDto[] { new SearchFilterDto(), new SearchFilterInternalDto() })
        {
            filter.Geographics = new GeographicsFilterDto
            {
                Geometries = [TextJsonSerializer.Deserialize<IGeometryObject>(geometryJson)!]
            };
            var job = CreateExportJob(filter);

            var restored = InvocationData.SerializeJob(job).DeserializeJob();

            Assert.Equal(job.Method, restored.Method);
            var restoredFilter = Assert.IsAssignableFrom<SearchFilterBaseDto>(restored.Args[0]);
            Assert.Equal(filter.GetType(), restoredFilter.GetType());
            var geometry = Assert.Single(restoredFilter.Geographics!.Geometries!);
            Assert.True(JsonNode.DeepEquals(JsonNode.Parse(geometryJson), TextJsonSerializer.SerializeToNode(geometry)));
        }
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"Geographics":{"Geometries":null}}""")]
    [InlineData("""{"Geographics":{"Geometries":[]}}""")]
    public void ExportJob_PreservesMissingNullAndEmptyGeometries(string filterJson)
    {
        var filter = JsonConvert.DeserializeObject<SearchFilterInternalDto>(filterJson)!;
        var restored = InvocationData.SerializeJob(CreateExportJob(filter)).DeserializeJob();
        var restoredFilter = Assert.IsType<SearchFilterInternalDto>(restored.Args[0]);

        Assert.Equal(filter.Geographics is null, restoredFilter.Geographics is null);
        Assert.Equal(filter.Geographics?.Geometries is null, restoredFilter.Geographics?.Geometries is null);
        Assert.Equal(filter.Geographics?.Geometries?.Count, restoredFilter.Geographics?.Geometries?.Count);
    }

    [Fact]
    public void StoredExportJob_DeserializesExistingMultiPolygonArgumentFormat()
    {
        // Fixed, anonymized argument data in the format written before the converter was introduced.
        const string filterJson = """
            {
              "$type":"SOS.Api.Contracts.Observations.Requests.SearchFilterInternalDto, SOS.Api.Contracts",
              "IncludeRealCount":true,
              "Output":{"Fields":["Occurrence.OccurrenceId"]},
              "ProtectionFilter":"BothPublicAndSensitive",
              "Geographics":{
                "IncludeObservationsOutsideSweden":true,
                "Geometries":[{
                  "$type":"GeoJSON.Text.Geometry.MultiPolygon, GeoJSON.Text",
                  "Type":"MultiPolygon",
                  "Coordinates":[{
                    "Type":"Polygon",
                    "Coordinates":[{
                      "Type":"LineString",
                      "Coordinates":[
                        {"$type":"GeoJSON.Text.Geometry.Position, GeoJSON.Text","Latitude":62,"Longitude":16},
                        {"$type":"GeoJSON.Text.Geometry.Position, GeoJSON.Text","Latitude":62,"Longitude":17},
                        {"$type":"GeoJSON.Text.Geometry.Position, GeoJSON.Text","Latitude":63,"Longitude":17},
                        {"$type":"GeoJSON.Text.Geometry.Position, GeoJSON.Text","Latitude":62,"Longitude":16}
                      ]
                    }]
                  }]
                }]
              }
            }
            """;
        const string parameterTypes = """
            ["SOS.Api.Contracts.Observations.Requests.SearchFilterBaseDto, SOS.Api.Contracts",
             "SOS.Api.Contracts.Observations.Requests.ProtectionFilterDto, SOS.Api.Contracts",
             "System.Int32","System.Nullable`1[[System.Int32]], mscorlib",
             "System.String","System.String","System.String",
             "SOS.Api.Contracts.Exports.Responses.ExportFormat, SOS.Api.Contracts",
             "System.String","System.Boolean",
             "SOS.Api.Contracts.Exports.Requests.PropertyLabelType, SOS.Api.Contracts",
             "System.Boolean","System.Boolean","System.String","System.Boolean",
             "Hangfire.Server.PerformContext, Hangfire.Core","Hangfire.IJobCancellationToken, Hangfire.Core"]
            """;
        string[] arguments =
        [
            filterJson, "\"BothPublicAndSensitive\"", "1", "15", "\"app\"", "\"user@example.org\"",
            "\"stored export\"", "\"Excel\"", "\"sv-SE\"", "false", "\"Swedish\"", "false", "false",
            "\"encrypted-password\"", "false", null!, null!
        ];
        var invocation = new InvocationData("SOS.Lib.Jobs.Export.IExportAndSendJob, SOS.Lib", "RunAsync",
            parameterTypes, JsonConvert.SerializeObject(arguments));

        var job = invocation.DeserializeJob();

        Assert.Equal(typeof(IExportAndSendJob).GetMethod("RunAsync"), job.Method);
        var filter = Assert.IsType<SearchFilterInternalDto>(job.Args[0]);
        Assert.True(filter.IncludeRealCount);
        Assert.Equal(["Occurrence.OccurrenceId"], filter.Output!.Fields);
        Assert.Equal(ProtectionFilterDto.BothPublicAndSensitive, filter.ProtectionFilter);
        Assert.True(filter.Geographics!.IncludeObservationsOutsideSweden);
        var geometry = Assert.IsType<MultiPolygon>(Assert.Single(filter.Geographics.Geometries!));
        var expected = JsonNode.Parse("""{"type":"MultiPolygon","coordinates":[[[[16,62],[17,62],[17,63],[16,62]]]]}""");
        Assert.True(JsonNode.DeepEquals(expected, TextJsonSerializer.SerializeToNode(geometry)));
        Assert.Equal(ProtectionFilterDto.BothPublicAndSensitive, job.Args[1]);
        Assert.Equal(15, job.Args[3]);
        Assert.Equal(ExportFormat.Excel, job.Args[7]);
        Assert.Equal(PropertyLabelType.Swedish, job.Args[10]);
    }

    [Fact]
    public void StoredDomainFilterJob_StillRoundTripsNetTopologySuiteGeometries()
    {
        var point = new NetTopologySuite.Geometries.Point(16, 62);
        var filter = new SearchFilter();
        filter.Location.Geometries = new GeographicsFilter { Geometries = [point] };
        var job = Job.FromExpression<IExportAndStoreJob>(j => j.RunAsync(filter, "exports", "observations", JobCancellationToken.Null));

        var restored = InvocationData.SerializeJob(job).DeserializeJob();

        var restoredFilter = Assert.IsType<SearchFilter>(restored.Args[0]);
        var restoredPoint = Assert.IsType<NetTopologySuite.Geometries.Point>(Assert.Single(restoredFilter.Location.Geometries.Geometries));
        Assert.Equal(point.X, restoredPoint.X);
        Assert.Equal(point.Y, restoredPoint.Y);
    }

    private static Job CreateExportJob(SearchFilterBaseDto filter) =>
        Job.FromExpression<IExportAndSendJob>(job => job.RunAsync(
            filter, ProtectionFilterDto.Public, 1, null, "app", "user@example.org", "export",
            ExportFormat.Excel, "sv-SE", false, PropertyLabelType.Swedish, false, false,
            null, false, null, JobCancellationToken.Null));
}
