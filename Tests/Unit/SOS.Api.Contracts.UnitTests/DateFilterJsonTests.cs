using FluentAssertions;
using SOS.Api.Contracts.Observations.Requests;
using System.Globalization;
using System.Text.Json;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class DateFilterJsonTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Theory]
    [InlineData("2000-02-29", 1, 1)]
    [InlineData("2026-03-29", 1, 2)]
    [InlineData("2026-10-25", 2, 1)]
    [InlineData(" 2026-01-01 ", 1, 1)]
    [InlineData("1/2/2026", 1, 1)]
    public void DateOnlyEndDate_IncludesTheSwedishDay_WithoutChangingOtherDateFields(string value, int startOffset, int endOffset)
    {
        var json = $$"""
            {
              "date": { "startDate": "{{value}}", "endDate": "{{value}}" },
              "modifiedDate": { "from": "{{value}}", "to": "{{value}}" },
              "extendedFilter": { "reportedDateFrom": "{{value}}", "reportedDateTo": "{{value}}" }
            }
            """;

        var filter = JsonSerializer.Deserialize<SearchFilterInternalDto>(json, JsonOptions)!;

        var date = DateTime.Parse(value, CultureInfo.InvariantCulture).Date;
        var start = new DateTimeOffset(date, TimeSpan.FromHours(startOffset));
        var end = new DateTimeOffset(date.AddHours(23).AddMinutes(59).AddSeconds(59), TimeSpan.FromHours(endOffset));
        filter.Date!.StartDate.Should().Be(start);
        filter.Date.EndDate.Should().Be(end);
        filter.Date.EndDate!.Value.Offset.Should().Be(end.Offset);
        filter.ModifiedDate!.From.Should().Be(start);
        filter.ModifiedDate.To.Should().Be(start);
        filter.ExtendedFilter!.ReportedDateFrom.Should().Be(start);
        filter.ExtendedFilter.ReportedDateTo.Should().Be(start);
    }

    [Theory]
    [InlineData("2026-01-01T00:00:00Z", "2026-01-01T00:00:00Z")]
    [InlineData("2026-01-01T00:00:00+02:00", "2026-01-01T00:00:00+02:00")]
    [InlineData("2026-01-01T00:00:00-05:00", "2026-01-01T00:00:00-05:00")]
    [InlineData("2026-01-01T12:34:56.789Z", "2026-01-01T12:34:56.789Z")]
    [InlineData("2026-01-01T00:00:00", "2026-01-01T00:00:00+01:00")]
    [InlineData("2026-07-01T12:34:56", "2026-07-01T12:34:56+02:00")]
    [InlineData("01/02/2026 12:34:56", "2026-01-02T12:34:56+01:00")]
    public void ExplicitEndTime_PreservesTimeAndOffset_WhenReadAndWritten(string value, string expectedValue)
    {
        var json = JsonSerializer.Serialize(new { endDate = value }, JsonOptions);
        var expected = DateTimeOffset.Parse(expectedValue, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces);

        var filter = JsonSerializer.Deserialize<DateFilterDto>(json, JsonOptions)!;
        var restored = JsonSerializer.Deserialize<DateFilterDto>(JsonSerializer.Serialize(filter, JsonOptions), JsonOptions)!;

        filter.EndDate.Should().Be(expected);
        filter.EndDate!.Value.Offset.Should().Be(expected.Offset);
        restored.EndDate.Should().Be(expected);
        restored.EndDate!.Value.Offset.Should().Be(expected.Offset);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"startDate\":null,\"endDate\":null}")]
    public void MissingOrNullDates_RemainNull(string json)
    {
        var filter = JsonSerializer.Deserialize<DateFilterDto>(json, JsonOptions)!;

        filter.StartDate.Should().BeNull();
        filter.EndDate.Should().BeNull();
    }

    [Theory]
    [InlineData("\"2026-06-31\"")]
    [InlineData("\"not-a-date\"")]
    [InlineData("42")]
    [InlineData("true")]
    public void InvalidEndDate_ThrowsJsonException_WithPropertyPath(string value)
    {
        var deserialize = () => JsonSerializer.Deserialize<DateFilterDto>($$"""{"endDate":{{value}}}""", JsonOptions);

        deserialize.Should().Throw<JsonException>().Which.Path.Should().Be("$.endDate");
    }
}
