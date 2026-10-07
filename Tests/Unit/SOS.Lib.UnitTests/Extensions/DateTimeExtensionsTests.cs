using SOS.Lib.Extensions;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class DateTimeExtensionsTests
{
    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ToUtcDateTimeOffset_PreservesUtcStorageDatesAndTheirBoundaries(DateTimeKind kind)
    {
        foreach (var value in new[] { DateTime.MinValue, DateTime.MaxValue, new DateTime(2026, 9, 30, 12, 34, 0) })
        {
            var result = DateTime.SpecifyKind(value, kind).ToUtcDateTimeOffset();

            Assert.Equal(value.Ticks, result.UtcTicks);
            Assert.Equal(TimeSpan.Zero, result.Offset);
        }
    }

    [Fact]
    public void ToUtcDateTimeOffset_PreservesLocalInstant()
    {
        var value = new DateTime(2026, 9, 30, 12, 34, 0, DateTimeKind.Local);

        var result = value.ToUtcDateTimeOffset();

        Assert.Equal(value.ToUniversalTime(), result.UtcDateTime);
        Assert.Equal(TimeSpan.Zero, result.Offset);
    }

    [Fact]
    public void ToUtcDateTimeOffset_PreservesNull()
    {
        Assert.Null(((DateTime?)null).ToUtcDateTimeOffset());
    }

    [Fact]
    public void ProjectDto_PreservesNullDates()
    {
        var result = new ProjectInfo().ToDto();

        Assert.Null(result.StartDate);
        Assert.Null(result.EndDate);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ProjectDto_UsesUtcStorageDatesWithoutLocalTimeInference(DateTimeKind kind)
    {
        var result = new ProjectInfo
        {
            StartDate = DateTime.SpecifyKind(DateTime.MinValue, kind),
            EndDate = DateTime.SpecifyKind(DateTime.MaxValue, kind)
        }.ToDto();

        Assert.Equal(DateTimeOffset.MinValue, result.StartDate);
        Assert.Equal(DateTimeOffset.MaxValue, result.EndDate);
        Assert.Equal(TimeSpan.Zero, result.StartDate!.Value.Offset);
        Assert.Equal(TimeSpan.Zero, result.EndDate!.Value.Offset);
    }
}
