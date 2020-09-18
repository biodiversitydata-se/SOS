namespace SOS.Observations.Api.Dtos
{
    public class LatLonBoundingBoxDto
    {
        public LatLonCoordinateDto BottomRight { get; set; }
        public LatLonCoordinateDto TopLeft { get; set; }
    }
}