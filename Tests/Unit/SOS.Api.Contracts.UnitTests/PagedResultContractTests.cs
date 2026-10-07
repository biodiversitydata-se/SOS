using System.Text.Json;
using SOS.Api.Contracts.Common.Responses;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class PagedResultContractTests
{
    [Fact]
    public void GeoPagedResultWithMetadataDto_PreservesTheJsonShape()
    {
        var result = new GeoPagedResultWithMetadataDto<int>
        {
            Skip = 2,
            Take = 5,
            TotalCount = 9,
            Records = [42]
        };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result));
        var root = document.RootElement;

        Assert.Equal(2, root.GetProperty("Skip").GetInt32());
        Assert.Equal(5, root.GetProperty("Take").GetInt32());
        Assert.Equal(9, root.GetProperty("TotalCount").GetInt64());
        Assert.Equal(42, root.GetProperty("Records")[0].GetInt32());
        Assert.False(root.TryGetProperty("GeoJson", out _));
        Assert.False(root.TryGetProperty("Metadata", out _));
    }

    [Fact]
    public void PagedResultDto_RoundTripsRecordsAsJsonArray()
    {
        var result = new PagedResultDto<int> { Records = [42, 43] };

        var json = JsonSerializer.Serialize(result);
        var deserialized = JsonSerializer.Deserialize<PagedResultDto<int>>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Records.Count);
        Assert.Equal(42, deserialized.Records[0]);
        Assert.Equal(43, deserialized.Records[1]);

        using var document = JsonDocument.Parse(json);
        Assert.Equal(JsonValueKind.Array, document.RootElement.GetProperty("Records").ValueKind);
    }

    [Fact]
    public void GeoJsonOnlyResult_LeavesRecordsNullAsBefore()
    {
        var result = new GeoPagedResultDto<int> { GeoJson = "{}" };

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(result));

        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("Records").ValueKind);
    }

    [Theory]
    [InlineData("{}", true)]
    [InlineData("{\"records\":null}", true)]
    [InlineData("{\"records\":[]}", false)]
    public void GeoPagedResultWithMetadataDto_PreservesMissingNullAndEmptyRecords(string json, bool expectNull)
    {
        var result = JsonSerializer.Deserialize<GeoPagedResultWithMetadataDto<int>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;

        if (expectNull) Assert.Null(result.Records);
        else Assert.Empty(result.Records);
    }
}
