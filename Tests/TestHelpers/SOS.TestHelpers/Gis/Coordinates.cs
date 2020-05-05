namespace SOS.TestHelpers.Gis
{
    /// <summary>
    /// Coordinates used for testing purpose.
    /// </summary>
    /// <remarks>
    /// Longitude = X
    /// Latitude = Y
    /// </remarks>
    public static class Coordinates
    {
        public static (
            double Latitude, 
            double Longitude, 
            double RT90X, 
            double RT90Y, 
            double Sweref99TmX, 
            double Sweref99TmY,
            double WebMercatorX,
            double WebMercatorY,
            double Etrs89X,
            double Etrs89Y) TranasMunicipality = (
                Latitude: 58.01539, 
                Longitude: 14.98996,
                RT90X: 1451813.85,
                RT90Y: 6432656.65, 
                Sweref99TmX: 499406.80,
                Sweref99TmY: 6430423.62,
                WebMercatorX: 1668674.71,
                WebMercatorY: 7970551.19,
                Etrs89X: 4616103.41,
                Etrs89Y: 3889622.99);
        public static (double Latitude, double Longitude) KirunaMunicipality = (Latitude: 67.85358, Longitude: 20.23455);
        public static (double Latitude, double Longitude) BorgholmMunicipality = (Latitude: 56.87874, Longitude: 16.697876);
        public static (double Latitude, double Longitude) KalmarMunicipality = (Latitude: 56.664722, Longitude: 16.366944);
        public static (double Latitude, double Longitude) FalkenbergMunicipality = (Latitude: 56.901389, Longitude: 12.497222);
    }
}
