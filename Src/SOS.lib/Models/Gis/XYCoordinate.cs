namespace SOS.Lib.Models.Gis
{
    public class XYCoordinate
    {
        public XYCoordinate(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }
    }
}