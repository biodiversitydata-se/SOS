using FluentAssertions;
using SOS.Api.Client.Requests;
using System.Globalization;
using Xunit;

namespace SOS.Api.Client.UnitTests.Requests;

public class AdaptiveSearchLocationsOptionsTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(2, "2")]
    [InlineData(2.5, "2.5")]
    public void PixelClusteringCutoff_IsSerializedWithInvariantCulture(double cutoff, string expected)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
            var options = new AdaptiveSearchLocationsOptions { SwitchToDetailClusteringAtMetersPerPixel = cutoff, DetailClusterRadiusPx = cutoff };
            options.ToQueryString().Split('&').Should().Contain($"switchToDetailClusteringAtMetersPerPixel={expected}");
            options.ToQueryString().Split('&').Should().Contain($"detailClusterRadiusPx={expected}");
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public void ClusteringDefaults_MatchMapSettingsAndAreSerialized()
    {
        var options = new AdaptiveSearchLocationsOptions();
        options.SwitchToDetailClusteringAtMetersPerPixel.Should().Be(2.0);
        options.ClusterRadiusPx.Should().Be(20.0);
        options.DetailClusterRadiusPx.Should().Be(10.0);
        options.MinLocationCountBeforeClustering.Should().Be(0);
        var query = options.ToQueryString().Split('&');
        query.Should().Contain(FormattableString.Invariant($"switchToDetailClusteringAtMetersPerPixel={options.SwitchToDetailClusteringAtMetersPerPixel}"));
        query.Should().Contain(FormattableString.Invariant($"clusterRadiusPx={options.ClusterRadiusPx}"));
        query.Should().Contain(FormattableString.Invariant($"detailClusterRadiusPx={options.DetailClusterRadiusPx}"));
        query.Should().Contain(FormattableString.Invariant($"minLocationCountBeforeClustering={options.MinLocationCountBeforeClustering}"));
    }
}
