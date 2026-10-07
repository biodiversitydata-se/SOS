using SOS.Api.Contracts.Aggregations.Requests;
using SOS.Api.Contracts.Aggregations.Responses;
using SOS.Api.Contracts.Checklists.Responses;
using SOS.Api.Contracts.DataQuality.Responses;
using SOS.Api.Contracts.Exports.Responses;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class ReportAndExportContractTests
{
    [Fact]
    public void TaxonTrendResult_PreservesJsonPropertiesAndValues()
    {
        var result = new TaxonTrendResult
        {
            TaxonId = 100, Quotient = 0.75, NrPresentObservations = 3,
            NrAbsentObservations = 1, NrChecklists = 4
        };

        AssertJson("""{"taxonId":100,"quotient":0.75,"nrPresentObservations":3,"nrAbsentObservations":1,"nrChecklists":4}""", result);
    }

    [Fact]
    public void DataQualityReport_PreservesNestedJsonAndStringIdentifiers()
    {
        var result = new DataQualityReport
        {
            Records = [new DataQualityReportRecord
            {
                EndDate = "2026-06-30", Locality = "Test", StartDate = "1990-01-31",
                TaxonId = "100", TaxonScientificName = "Test taxon", UniqueKey = "key",
                Observations = [new DataQualityReportObservation { DataProviderId = "1", OccurrenceId = "occ-1" }]
            }]
        };

        AssertJson("""
            {"records":[{"endDate":"2026-06-30","locality":"Test","observations":[{"dataProviderId":"1","occurrenceId":"occ-1"}],
              "startDate":"1990-01-31","taxonId":"100","taxonScientificName":"Test taxon","uniqueKey":"key"}]}
            """, result);
    }

    [Fact]
    public void DataQualityReport_PreservesEmptyRecordsAndNullFields()
    {
        AssertJson("""{"records":[]}""", new DataQualityReport());
        AssertJson("""
            {"endDate":null,"locality":null,"observations":null,"startDate":null,"taxonId":null,"taxonScientificName":null,"uniqueKey":null}
            """, new DataQualityReportRecord());
    }

    [Fact]
    public void ExportFile_PreservesDateTimeAndLongFileSize()
    {
        AssertJson("""{"name":"dataset.zip","created":"2026-01-02T03:04:05Z","size":3000000000,"downloadUrl":"https://example.org/dataset.zip"}""",
            new ExportFileDto
            {
                Name = "dataset.zip", Created = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                Size = 3_000_000_000, DownloadUrl = "https://example.org/dataset.zip"
            });
    }

    [Fact]
    public void TaxonAreaAggregation_PreservesDictionaryKeysNullAndEmptyAreas()
    {
        AssertJson("""
            {"100":{"observationCount":3,"sumObservationCount":5,"observationCountByAreaFeatureId":null,"sumObservationCountByAreaFeatureId":{}}}
            """, new Dictionary<int, TaxonAreaAggregation>
            {
                [100] = new() { ObservationCount = 3, SumObservationCount = 5, SumObservationCountByAreaFeatureId = [] }
            });
    }

    [Theory]
    [InlineData(AggregationSortOrder.CountDescending, 0, "CountDescending")]
    [InlineData(AggregationSortOrder.CountAscending, 1, "CountAscending")]
    [InlineData(AggregationSortOrder.KeyAscending, 2, "KeyAscending")]
    [InlineData(AggregationSortOrder.KeyDescending, 3, "KeyDescending")]
    [InlineData(AggregationSortOrder.OrganismQuantityAscending, 4, "OrganismQuantityAscending")]
    [InlineData(AggregationSortOrder.OrganismQuantityDescending, 5, "OrganismQuantityDescending")]
    public void AggregationSortOrder_PreservesNamesNumericValuesAndStringSerialization(AggregationSortOrder value, int number, string name)
    {
        Assert.Equal(number, (int)value);
        Assert.Equal(name, value.ToString());
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        Assert.Equal($"\"{name}\"", JsonSerializer.Serialize(value, options));
        Assert.Equal(value, JsonSerializer.Deserialize<AggregationSortOrder>($"\"{name}\"", options));
    }

    private static void AssertJson<T>(string expected, T actual)
    {
        using var expectedJson = JsonDocument.Parse(expected);
        using var actualJson = JsonDocument.Parse(JsonSerializer.Serialize(actual, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.True(JsonElement.DeepEquals(expectedJson.RootElement, actualJson.RootElement), actualJson.RootElement.ToString());
    }
}
