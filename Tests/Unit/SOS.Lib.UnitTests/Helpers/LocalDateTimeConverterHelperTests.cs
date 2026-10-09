using SOS.Lib.Helpers;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers;

public class LocalDateTimeConverterHelperTests
{
    [Theory]
    [InlineData("2026-01-15T10:00:00Z", "2026-01-15T11:00:00+01:00")]
    [InlineData("2026-07-15T05:00:00-05:00", "2026-07-15T12:00:00+02:00")]
    [InlineData("2026-07-15T10:00:00", "2026-07-15T12:00:00+02:00")]
    public void ObservationDates_UseSwedishOffset_AndRepeatedConversionPreservesTheInstant(string timestamp, string expected)
    {
        var observation = JsonNode.Parse($$$"""
            {"created":"{{{timestamp}}}","modified":"{{{timestamp}}}",
             "event":{"startDate":"{{{timestamp}}}","endDate":"{{{timestamp}}}"},
             "occurrence":{"reportedDate":"{{{timestamp}}}"}}
            """)!;

        LocalDateTimeConverterHelper.ConvertToLocalTime(new[] { observation });
        var firstResponse = observation.ToJsonString();
        LocalDateTimeConverterHelper.ConvertToLocalTime(new[] { observation });

        Assert.Equal(firstResponse, observation.ToJsonString());
        using var json = JsonDocument.Parse(firstResponse);
        Assert.Equal(expected, json.RootElement.GetProperty("created").GetString());
        Assert.Equal(expected, json.RootElement.GetProperty("modified").GetString());
        Assert.Equal(expected, json.RootElement.GetProperty("event").GetProperty("startDate").GetString());
        Assert.Equal(expected, json.RootElement.GetProperty("event").GetProperty("endDate").GetString());
        Assert.Equal(expected, json.RootElement.GetProperty("occurrence").GetProperty("reportedDate").GetString());
    }

    [Fact]
    public void MissingAndNullDates_AreNotAddedOrChanged()
    {
        var observation = JsonNode.Parse("""{"modified":null,"event":{"startDate":null},"occurrence":{}}""")!;
        var original = observation.ToJsonString();

        LocalDateTimeConverterHelper.ConvertToLocalTime(new[] { observation });

        Assert.Equal(original, observation.ToJsonString());
    }
}
