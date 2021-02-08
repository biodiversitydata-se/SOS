using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos
{
    public class GeoGridTileTaxaCellDto
    {
        public LatLonBoundingBoxDto BoundingBox { get; set; }
        public string GeoTile { get; set; }
        public int Zoom { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public IEnumerable<GeoGridTileTaxonObservationCountDto> Taxa { get; set; }
    }
}