namespace SOS.TestHelpers.Gis
{
    /// <summary>
    ///     Coordinates used for testing purpose.
    /// </summary>
    /// <remarks>
    ///     Longitude = X
    ///     Latitude = Y
    /// </remarks>
    public static class TestCoordinates
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

        public static (double Latitude, double Longitude)
            KirunaMunicipality = (Latitude: 67.85358, Longitude: 20.23455);

        public static (double Latitude, double Longitude) BorgholmMunicipality =
            (Latitude: 56.87874, Longitude: 16.697876);

        public static (double Latitude, double Longitude) KalmarMunicipality =
            (Latitude: 56.664722, Longitude: 16.366944);

        public static (double Latitude, double Longitude) FalkenbergMunicipality =
            (Latitude: 56.901389, Longitude: 12.497222);

        public static (double Latitude, double Longitude, double WebMercatorX, double WebMercatorY) UppsalaMunicipality = (
            Latitude: 59.8586, 
            Longitude: 17.6389,
            WebMercatorX: 1963553.37,
            WebMercatorY: 8368323.80);

        public static (double TopLeftLatitude, double TopLeftLongitude, double BottomRightLatitude, double BottomRightLongitude) UppsalaMunicipalityBbox = (
            TopLeftLatitude: 60.1,
            TopLeftLongitude: 17.3,
            BottomRightLatitude: 59.65,
            BottomRightLongitude: 18.05);

        public static (double Latitude, double Longitude, double WebMercatorX, double WebMercatorY) TierpMunicipality = (
            Latitude: 60.3433,
            Longitude: 17.5151,
            WebMercatorX: 1954632.89,
            WebMercatorY: 8420675.02);

        public static (double TopLeftLatitude, double TopLeftLongitude, double BottomRightLatitude, double BottomRightLongitude) TierpCenterBbox = (
            TopLeftLatitude: 60.3433 + 0.009,
            TopLeftLongitude: 17.5151 - 0.018,
            BottomRightLatitude: 60.3433 - 0.009,
            BottomRightLongitude: 17.5151 + 0.018);

        public static (double TopLeftLatitude, double TopLeftLongitude, double BottomRightLatitude, double BottomRightLongitude) TierpMunicipalityBbox = (
            TopLeftLatitude: 60.5,
            TopLeftLongitude: 17.1,
            BottomRightLatitude: 60.1,
            BottomRightLongitude: 17.9);

        public static (double Latitude, double Longitude, double WebMercatorX, double WebMercatorY) JönköpingMunicipality = (
            Latitude: 57.7826,
            Longitude: 14.1618,
            WebMercatorX: 1576484.36,
            WebMercatorY: 7921786.57);

        public static (double TopLeftLatitude, double TopLeftLongitude, double BottomRightLatitude, double BottomRightLongitude) JönköpingMunicipalityBbox = (
            TopLeftLatitude: 57.95,
            TopLeftLongitude: 13.75,
            BottomRightLatitude: 57.60,
            BottomRightLongitude: 14.50);
    }
}