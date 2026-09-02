using SOS.Api.Contracts.Common.Geo;

namespace SOS.Api.Contracts.Aggregations.Responses;
public class GeoGridCellDto
{

    public long? ObservationsCount { get; set; }

    public long? TaxaCount { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Zoom { get; set; }

    public LatLonBoundingBoxDto BoundingBox { get; set; }

}


