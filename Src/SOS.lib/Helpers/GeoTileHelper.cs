using System;
using System.Collections.Generic;
using System.Text;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Helpers
{
    public static class GeoTileHelper
    {
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
}
