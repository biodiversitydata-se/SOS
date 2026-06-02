namespace SOS.Observations.Api.IntegrationTests.Models;

public class GeoJsonFlatObservationsByLocation
{
    public GeoJsonFlatLocation Location { get; set; } = null!;
    public int ObservationCount { get; set; }
    public int TaxaCount { get; set; }
    public IEnumerable<GeoJsonFlatObservationNoLocation> Observations { get; set; } = null!;
}