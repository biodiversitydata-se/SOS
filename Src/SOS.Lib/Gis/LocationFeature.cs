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
    /// When true, the Location and Geometry properties represent the centroid of the cluster, computed as the average
    /// of member latitudes and longitudes. The Locations property contains the list of original location objects that
    /// are grouped into the cluster. When false, the Location and Geometry properties represent the single location,
    /// and the Locations property is null.
    /// </summary>
    public bool IsCluster { get; set; }

    /// <summary>
    /// If IsCluster is false, this property represents the single location associated with the observation(s). For
    /// clusters, this property represents the centroid location of the cluster.
    /// </summary>
    public SOS.Lib.Models.Processed.Observation.Location? Location { get; set; }

    /// <summary>
    /// The geometry of the location or cluster. For a single location, this is a Point geometry or Polygon geometry.
    /// Clustered locations will use the centroid point as the geometry for the cluster, which is computed as the
    /// average of member latitudes and longitudes.
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
    /// Indicates the number of distinct original locations that are
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
    /// Gets the centroid latitude for this item.
    /// </summary>
    public double CentroidLatitude => Locations.Average(l => l.DecimalLatitude!.Value);

    /// <summary>
    /// Gets the centroid longitude for this item.
    /// </summary>
    public double CentroidLongitude => Locations.Average(l => l.DecimalLongitude!.Value);

    /// <summary>
    /// The counts of observations in different status categories (found, not found, treated, not treated, diffused, not
    /// diffused).
    /// </summary>
    public ObservationStatusCounts StatusCounts { get; set; }

    /// <summary>
    /// True when the item was created from representative observations instead of all observations for the location.
    /// </summary>
    public bool? IsSummaryApproximated { get; set; }

    /// <summary>
    /// Creates a clustered item.
    /// </summary>
    /// <param name="observations">
    /// The clustered observations.
    /// </param>
    /// <returns>
    /// A clustered location item.
    /// </returns>
    internal static LocationFeature FromCluster(List<Observation> observations)
    {
        var locations = observations
            .Select(o => o.Location)
            .GroupBy(location => location.DistinctLocationId)
            .Select(group => group.First())
            .ToList();
        var centroidLatitude = locations.Average(l => l.DecimalLatitude!.Value);
        var centroidLongitude = locations.Average(l => l.DecimalLongitude!.Value);
        var statusCounts = CreateStatusCounts(observations);

        return new LocationFeature
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
        var location = observations.First().Location;
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

    private static ObservationStatusCounts CreateStatusCounts(List<Observation> observations)
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

        return new ObservationStatusCounts(
            foundCount,
            treatedCount,
            diffusedCount,
            redlistedCount,
            invasiveInSwedenCount);
    }
}
