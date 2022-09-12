namespace SOS.Analysis.Api.Dtos.Filter
{
    public class LatLonBoundingBoxDto
    {
        public LatLonCoordinateDto BottomRight { get; set; }
        public LatLonCoordinateDto TopLeft { get; set; }
    }
}
