namespace SOS.Shared.Api.Dtos
{
    public class XYBoundingBoxDto
    {
        public XYCoordinateDto BottomRight { get; set; }
        public XYCoordinateDto TopLeft { get; set; }
    }
}