namespace SOS.Observations.Api.Dtos
{
    public class XYBoundingBoxDto
    {
        public XYCoordinateDto BottomRight { get; set; }
        public XYCoordinateDto TopLeft { get; set; }
    }
}