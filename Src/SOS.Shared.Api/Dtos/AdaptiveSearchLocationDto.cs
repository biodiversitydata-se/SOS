using NetTopologySuite.Geometries;
using SOS.Lib.Gis;

namespace SOS.Shared.Api.Dtos;

public class AdaptiveSearchLocationDto<TObservation, TLocation>
{
    public TLocation Location { get; set; }
    public Geometry Geometry { get; set; }    
    public int ObservationCount { get; set; }
    public int TaxaCount { get; set; }
    public int LocationCount { get; set; } = 1;    
    public ObservationStatusCounts? StatusCounts { get; set; }
    public IEnumerable<TObservation> Observations { get; set; } = null!;
}
