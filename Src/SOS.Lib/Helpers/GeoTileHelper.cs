using SOS.Lib.Models.Gis;

namespace SOS.Lib.Helpers;

public static class GeoTileHelper
{    
    private const double MaxLatLon = 180.0;
    private const int MaxElasticsearchZoom = 29;

    public static int CalculateGeotileZoom(
        LatLonBoundingBox bbox,
        int maxBuckets,
        int maxZoom)
    {
        // 1. Calculate width and height in degrees
        double width = Math.Abs(bbox.TopLeft.Longitude - bbox.BottomRight.Longitude);
        double height = Math.Abs(bbox.TopLeft.Latitude - bbox.BottomRight.Latitude);

        // Handle date line crossing or global bounds
        if (width > MaxLatLon)
        {
            width = 360.0;
        }

        // 2. Estimate the required density
        // Compare the bbox area to the total world area (simplified 360x180 degrees)
        double totalWorldArea = 360.0 * 180.0;
        double bboxArea = width * height;

        // Extrapolate: If this density was applied globally, how many tiles would we need?
        double maxTilesForFullArea = maxBuckets * totalWorldArea / bboxArea;

        // 3. Calculate Zoom (Z)
        // The total number of tiles at zoom Z is 4^Z. We solve for Z:
        double zoomFloat = Math.Log(maxTilesForFullArea, 4);
        int optimalZoom = (int)Math.Floor(zoomFloat);

        // Ensure zoom is within bounds
        // Clamp between 0 and the user-provided maxZoom (capped at Elastic's absolute max)
        return Math.Max(0, Math.Min(optimalZoom, Math.Min(maxZoom, MaxElasticsearchZoom)));
    }

    /// <summary>
    /// Calculates the maximum geotile zoom such that grid cell count is less than or equals maxGridCells.
    /// The result is capped by maxZoom.
    /// </summary>
    /// <remarks>
    /// For some parameters this method returns lower zoom levels than CalculateGeotileZoom
    /// </remarks>
    public static int CalculateGeotileZoomUsingWebMercator(LatLonBoundingBox extent, int maxGridCells, int maxZoom)
    {
        if (extent == null)
            return 1;

        // Web Mercator world extent
        const double MinLat = -85.05112878;
        const double MaxLat = 85.05112878;

        // Clamp coordinates to Web Mercator valid range
        double left = extent.TopLeft.Longitude;
        double right = extent.BottomRight.Longitude;
        double top = Math.Min(extent.TopLeft.Latitude, MaxLat);
        double bottom = Math.Max(extent.BottomRight.Latitude, MinLat);

        // Convert to Web Mercator meters
        (double mx1, double my1) = ToWebMercator(left, top);
        (double mx2, double my2) = ToWebMercator(right, bottom);

        double width = Math.Abs(mx2 - mx1);
        double height = Math.Abs(my2 - my1);

        if (width == 0 || height == 0)
            return 1;

        const double EarthExtent = 20037508.342789244 * 2; // full WebMercator world width in meters

        int bestZoom = 1;

        for (int z = 1; z <= maxZoom; z++)
        {
            double scale = Math.Pow(2, z);

            int tilesX = (int)Math.Ceiling(width / (EarthExtent / scale));
            int tilesY = (int)Math.Ceiling(height / (EarthExtent / scale));
            int totalTiles = tilesX * tilesY;

            if (totalTiles <= maxGridCells)
            {
                bestZoom = z;  // keep increasing as long as it fits
            }
            else
            {
                break; // going too detailed → stop
            }
        }

        return bestZoom;
    }

    private static (double x, double y) ToWebMercator(double lon, double lat)
    {
        double x = lon * 20037508.34 / 180.0;
        double y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360.0)) / (Math.PI / 180.0);
        y = y * 20037508.34 / 180.0;
        return (x, y);
    }

    public static LatLonCoordinate GetCoordinateFromTile(int x, int y, int zoom)
    {
        return new LatLonCoordinate(TileY2Lat(y, zoom), TileX2Long(x, zoom));
    }

    public static (int zoom, int x, int y) GetZoomAndCoordinatesFromKey(string key)
    {
        var parts = key.Split('/');
        return (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    }

    private static int Long2TileX(double lon, int z)
    {
        return (int)(Math.Floor((lon + 180.0) / 360.0 * (1 << z)));
    }

    private static int Lat2TileY(double lat, int z)
    {

        return (int)Math.Floor((1 - Math.Log(Math.Tan(ToRadians(lat)) + 1 / Math.Cos(ToRadians(lat))) / Math.PI) / 2 * (1 << z));
    }

    private static double TileX2Long(int x, int z)
    {
        var res = x / (double)(1 << z) * 360.0 - 180;
        return res;
    }

    private static double TileY2Lat(int y, int z)
    {
        double n = Math.PI - 2.0 * Math.PI * y / (double)(1 << z);
        var res = 180.0 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)));
        return res;
    }

    private static double ToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }

    private static double ToDegrees(double radians)
    {
        return radians * 180f / (float)Math.PI;
    }
}
