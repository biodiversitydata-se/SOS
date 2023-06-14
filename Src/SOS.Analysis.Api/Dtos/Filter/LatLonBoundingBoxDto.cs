using NetTopologySuite.Geometries;

namespace SOS.Analysis.Api.Dtos.Filter
{
    public class LatLonBoundingBoxDto
    {
        public static LatLonBoundingBoxDto? Create(Envelope envelope)
        {
            if (envelope == null)
            {
                return null;
            }

            return new LatLonBoundingBoxDto
            {
                TopLeft = new LatLonCoordinateDto { Latitude = envelope.MaxY, Longitude = envelope.MinX },
                BottomRight = new LatLonCoordinateDto { Latitude = envelope.MinY, Longitude = envelope.MaxX }
            };
        }

        public LatLonCoordinateDto? BottomRight { get; set; }
        public LatLonCoordinateDto? TopLeft { get; set; }

        public Envelope ToEnvelope()
        {
            return new Envelope(
                new Coordinate(TopLeft?.Longitude ?? 0, TopLeft?.Latitude ?? 0), 
                new Coordinate(BottomRight?.Longitude ?? 0, BottomRight?.Latitude ?? 0)
            );
        }
    }
}
