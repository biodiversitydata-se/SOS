using SOS.Lib.Models.Processed.Observation;

namespace SOS.Observations.Api.IntegrationTests.Models;

public class ObservationsByLocation
{     
    public Location Location { get; set; } = null!;
    public int ObservationCount { get; set; }
    public int TaxaCount { get; set; }
    public IEnumerable<Observation> Observations { get; set; } = null!;
}
