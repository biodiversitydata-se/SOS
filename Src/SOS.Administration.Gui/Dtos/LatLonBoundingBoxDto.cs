namespace SOS.Administration.Gui.Dtos
{
    public class LatLonBoundingBoxDto
    {
        public LatLonCoordinateDto BottomRight { get; set; }
        public LatLonCoordinateDto TopLeft { get; set; }
    }
}