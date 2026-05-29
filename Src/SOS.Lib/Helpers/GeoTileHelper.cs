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

    public static LatLonCoordinate GetCoordinateFromTile(int x, int y, int zoom)
    {
        return new LatLonCoordinate(TileY2Lat(y, zoom), TileX2Long(x, zoom));
    }

    public static (int zoom, int x, int y) GetZoomAndCoordinatesFromKey(string key)
    {
        var parts = key.Split('/');
        return (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
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
}