using SOS.Api.Contracts.Common.Geo;
using LocationGroupingType = SOS.Api.Contracts.Observations.Responses.LocationGroupingType;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Gis;
using NetTopologySuite.Geometries;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Gis;

/// <summary>
/// Represents one item in a location clustering result.
/// </summary>
public class LocationFeature
{
    /// <summary>
    /// Indicates whether this result represents a cluster of multiple locations (true) or a single location (false).
    /// For FastGreedy, Location and Geometry use the SWEREF99 TM centroid of the underlying geographic groups.
    /// Locations retains those geographic groups; OriginalLocations retains metadata per original location id.
    /// </summary>
    public bool IsCluster { get; set; }

    public LocationGroupingType GroupingType { get; set; } = LocationGroupingType.PixelCluster;
    public List<SOS.Lib.Models.Processed.Observation.Location> OriginalLocations { get; set; } = new();
    public HashSet<string> GeneralizedLocationIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// If IsCluster is false, this property represents the single location associated with the observation(s). For
    /// clusters, this property represents the centroid location of the cluster.
    /// </summary>
    public SOS.Lib.Models.Processed.Observation.Location? Location { get; set; }

    /// <summary>
    /// The geometry of the location or cluster. For a single location, this is a Point geometry or Polygon geometry.
    /// Clustered locations use their centroid point as the geometry.
    /// </summary>
    public Geometry? Geometry { get; set; }        

    /// <summary>
    /// The number of observations for this item.
    /// </summary>
    public int ObservationCount { get; set; }

    /// <summary>
    /// The number of taxa for this item.
    /// </summary>
    public int TaxaCount { get; set; }

    /// <summary>
    /// Indicates the number of distinct geographic groups that are
    /// grouped into this cluster. Always 1 when IsCluster is false.    
    /// </summary>
    public int LocationCount { get; set; } = 1;

    /// <summary>
    /// The clustering radius in metres that was used when this cluster was created, when known.
    /// </summary>
    public double? ClusterRadiusMeters { get; set; }

    /// <summary>
    /// The observations that are associated with this location or cluster.
    /// </summary>
    public List<Observation> Observations { get; set; } = new();    

    /// <summary>
    /// A compact observation summary for this item, used when the item was created from aggregated location data.
    /// </summary>
    public LocationObservationSummary? ObservationSummary { get; set; }

    /// <summary>
    /// The distinct location ids represented by this item, when known without inspecting observations.
    /// </summary>
    public List<string> LocationIds { get; set; } = new();

    /// <summary>
    /// The locations represented by this item.
    /// </summary>
    public List<SOS.Lib.Models.Processed.Observation.Location> Locations { get; set; } = new();

    /// <summary>
    /// Gets the unprojected mean latitude (legacy algorithms). FastGreedy's metric centroid is in Location.
    /// </summary>
    public double CentroidLatitude => Locations.Average(l => l.DecimalLatitude!.Value);

    /// <summary>
    /// Gets the unprojected mean longitude (legacy algorithms). FastGreedy's metric centroid is in Location.
    /// </summary>
    public double CentroidLongitude => Locations.Average(l => l.DecimalLongitude!.Value);

    /// <summary>
    /// The counts of observations in different status categories (found, not found, treated, not treated, diffused, not
    /// diffused).
    /// </summary>
    public SOS.Api.Contracts.Observations.Responses.ObservationStatusCounts StatusCounts { get; set; }

    /// <summary>
    /// True when representative observations make summary information potentially approximate.
    /// Covers observationSummary, presentation.statusCounts and potentially
    /// presentation.isAllObservationsSensitive. Individual values may still be exact;
    /// observationCount remains the total matching count.
    /// </summary>
    public bool? IsSummaryApproximated { get; set; }

    /// <summary>
    /// True when presentation.status or presentation.cutoffYearStatus may differ from the complete
    /// observation result. Applies only to these two properties, not the entire presentation object.
    /// </summary>
    public bool IsPresentationStatusApproximated { get; set; }

    /// <summary>
    /// Creates a clustered item.
    /// </summary>
    /// <param name="observations">
    /// The clustered observations.
    /// </param>
    /// <param name="metricCenter">Optional precomputed SWEREF99 TM centroid.</param>
    /// <returns>
    /// A clustered location item.
    /// </returns>
    internal static LocationFeature FromCluster(List<Observation> observations, Point? metricCenter = null)
    {
        var locations = observations
            .Select(o => o.Location)
            .GroupBy(location => location.DistinctLocationId)
            .Select(group => group.First())
            .ToList();
        var centroidLatitude = locations.Average(l => l.DecimalLatitude!.Value);
        var centroidLongitude = locations.Average(l => l.DecimalLongitude!.Value);
        var statusCounts = CreateStatusCounts(observations);

        var result = new LocationFeature
        {
            IsCluster = true,
            LocationCount = locations.Count,
            ObservationCount = observations.Count,
            TaxaCount = observations.Select(observation => observation.Taxon?.Id).Distinct().Count(),
            Observations = observations,
            Locations = locations,
            StatusCounts = statusCounts,
            Location = new SOS.Lib.Models.Processed.Observation.Location
            {
                DecimalLatitude = centroidLatitude,
                DecimalLongitude = centroidLongitude
            },
            Geometry = new Point(centroidLongitude, centroidLatitude)
        };
        if (metricCenter != null) result.SetMetricCentroid(metricCenter);
        return result;
    }

    /// <summary>
    /// Creates a non-clustered item.
    /// </summary>
    /// <param name="observation">
    /// The single observation.
    /// </param>
    /// <returns>
    /// A non-clustered location item.
    /// </returns>
    public static LocationFeature FromSingle(Observation observation)
    {
        return FromSingleLocation(new List<Observation> { observation });
    }

    /// <summary>
    /// Creates a non-clustered item for one location with one or more observations.
    /// </summary>
    /// <param name="observations">
    /// The observations for the location.
    /// </param>
    /// <returns>
    /// A non-clustered location item.
    /// </returns>
    public static LocationFeature FromSingleLocation(List<Observation> observations)
    {
        var location = observations.OrderBy(o => o.Location.LocationId, StringComparer.Ordinal).First().Location;
        var geometry = location.Point ?? new Point(location.DecimalLongitude!.Value, location.DecimalLatitude!.Value);
        var statusCounts = CreateStatusCounts(observations);

        return new LocationFeature
        {
            IsCluster = false,
            Location = location,
            Geometry = geometry,
            LocationCount = 1,
            ObservationCount = observations.Count,
            TaxaCount = observations.Select(observation => observation.Taxon?.Id).Distinct().Count(),
            Observations = observations,
            Locations = new List<SOS.Lib.Models.Processed.Observation.Location> { location },
            StatusCounts = statusCounts
        };
    }

    /// <summary>Each distinct geographic position has equal weight, irrespective of observation count.</summary>
    public void SetMetricCentroid(Point? center = null)
    {
        if (center == null)
        {
            var points = Locations.GroupBy(l => l.DistinctLocationId).Select(g => ProjectLocation(g.First())).ToList();
            center = new Point(points.Average(p => p.X), points.Average(p => p.Y));
        }
        center.SRID = CoordinateSys.SWEREF99_TM.Srid();
        var wgs = (Point)center.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84, usePointCache: false);
        Location = new SOS.Lib.Models.Processed.Observation.Location
        {
            DecimalLongitude = wgs.X, DecimalLatitude = wgs.Y,
            Sweref99TmX = center.X, Sweref99TmY = center.Y
        };
        Geometry = wgs;
    }

    public static Point ProjectLocation(SOS.Lib.Models.Processed.Observation.Location location)
    {
        if (location.Sweref99TmX is double x && location.Sweref99TmY is double y && double.IsFinite(x) && double.IsFinite(y))
            return new Point(x, y) { SRID = CoordinateSys.SWEREF99_TM.Srid() };
        return (Point)new Point(location.DecimalLongitude!.Value, location.DecimalLatitude!.Value)
            .Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM, usePointCache: false);
    }

    private static SOS.Api.Contracts.Observations.Responses.ObservationStatusCounts CreateStatusCounts(List<Observation> observations)
    {
        int foundCount = 0;
        var diffusedCount = 0;
        var treatedCount = 0;
        var redlistedCount = 0;
        var invasiveInSwedenCount = 0;

        foreach (var observation in observations)
        {
            if (observation.Occurrence?.IsPositiveObservation == true)
            {
                foundCount++;
            }            

            if (observation.IsGeneralized)
            {
                diffusedCount++;
            }

            if ((observation.ArtportalenInternal?.InvasiveSpeciesTreatment?.Id ?? -1) >= 0)
            {
                treatedCount++;
            }

            if (observation.Taxon?.Attributes?.IsRedlisted == true)
            {
                redlistedCount++;
            }

            if (observation.Taxon?.Attributes?.IsInvasiveInSweden == true)
            {
                invasiveInSwedenCount++;
            }
        }

        return new SOS.Api.Contracts.Observations.Responses.ObservationStatusCounts(
            foundCount,
            treatedCount,
            diffusedCount,
            redlistedCount,
            invasiveInSwedenCount);
    }
}
