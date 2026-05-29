using System.Globalization;

namespace SOS.Observations.Api.Services.AdaptiveSearchLocations;

public sealed class AdaptiveSearchLocationsTimings
{
    public TimeSpan CacheLookup { get; set; }

    public TimeSpan ElasticsearchCountAndExtent { get; set; }

    public TimeSpan ElasticsearchObservations { get; set; }

    public TimeSpan ElasticsearchGeogrid { get; set; }

    public TimeSpan PolygonGeometries { get; set; }

    public TimeSpan Clustering { get; set; }

    public string ToHeaderValue(TimeSpan total)
    {
        var elasticsearch = ElasticsearchCountAndExtent + ElasticsearchObservations + ElasticsearchGeogrid;
        var measured = CacheLookup + elasticsearch + PolygonGeometries + Clustering;
        var other = total - measured;
        if (other < TimeSpan.Zero)
        {
            other = TimeSpan.Zero;
        }

        return string.Join(
            "; ",
            $"total={Format(total)}",
            $"elasticsearch={Format(elasticsearch)}",
            $"countExtent={Format(ElasticsearchCountAndExtent)}",
            $"observations={Format(ElasticsearchObservations)}",
            $"geogrid={Format(ElasticsearchGeogrid)}",
            $"cache={Format(CacheLookup)}",
            $"polygons={Format(PolygonGeometries)}",
            $"clustering={Format(Clustering)}",
            $"other={Format(other)}");
    }

    private static string Format(TimeSpan value)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{value.TotalMilliseconds:0.#}ms");
    }
}
