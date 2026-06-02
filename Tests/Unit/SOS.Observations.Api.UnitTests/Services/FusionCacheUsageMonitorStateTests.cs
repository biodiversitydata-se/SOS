using FluentAssertions;
using SOS.Observations.Api.Services;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class FusionCacheUsageMonitorStateTests
{
    [Fact]
    public void TrackWrite_Should_Count_Writes_Keys_And_SizeUnits()
    {
        var state = new FusionCacheUsageMonitorState("test-cache", 100);

        var snapshot = state.TrackWrite("key-1", 10);

        snapshot.TotalWrites.Should().Be(1);
        snapshot.TrackedCacheKeys.Should().Be(1);
        snapshot.UsedSizeUnits.Should().Be(10);
        snapshot.SizeLimitUnits.Should().Be(100);
        snapshot.UsagePercent.Should().Be(10);
    }

    [Fact]
    public void TrackWrite_Should_Update_SizeUnits_When_Key_Is_Replaced()
    {
        var state = new FusionCacheUsageMonitorState("test-cache", 100);
        state.TrackWrite("key-1", 10);

        var snapshot = state.TrackWrite("key-1", 25);

        snapshot.TotalWrites.Should().Be(2);
        snapshot.TrackedCacheKeys.Should().Be(1);
        snapshot.UsedSizeUnits.Should().Be(25);
        snapshot.UsagePercent.Should().Be(25);
    }

    [Fact]
    public void TrackCapacityEviction_Should_Remove_Key_And_Decrease_SizeUnits()
    {
        var state = new FusionCacheUsageMonitorState("test-cache", 100);
        state.TrackWrite("key-1", 10);
        state.TrackWrite("key-2", 20);

        var snapshot = state.TrackCapacityEviction("key-1");

        snapshot.CapacityEvictions.Should().Be(1);
        snapshot.TrackedCacheKeys.Should().Be(1);
        snapshot.UsedSizeUnits.Should().Be(20);
        snapshot.EntrySizeUnits.Should().Be(10);
        snapshot.Key.Should().Be("key-1");
    }

    [Fact]
    public void TrackCapacityEviction_Should_Match_Alternate_Key()
    {
        var state = new FusionCacheUsageMonitorState("test-cache", 100);
        state.TrackWrite("key-1", 10);

        var snapshot = state.TrackCapacityEviction("test-cache:key-1", new[] { "key-1" });

        snapshot.TrackedCacheKeys.Should().Be(0);
        snapshot.UsedSizeUnits.Should().Be(0);
        snapshot.EntrySizeUnits.Should().Be(10);
        snapshot.Key.Should().Be("key-1");
    }

    [Fact]
    public void ShouldLogPeriodicWrite_Should_Return_True_For_Configured_Interval()
    {
        var state = new FusionCacheUsageMonitorState("test-cache", 100);
        state.TrackWrite("key-1", 10);
        var snapshot = state.TrackWrite("key-2", 20);

        FusionCacheUsageMonitorState.ShouldLogPeriodicWrite(snapshot, 2).Should().BeTrue();
        FusionCacheUsageMonitorState.ShouldLogPeriodicWrite(snapshot, 3).Should().BeFalse();
    }
}
