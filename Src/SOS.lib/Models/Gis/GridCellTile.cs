using System;
using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;

namespace SOS.Lib.Models.Gis
{
    public class GridCellTile
    {
        public long? ObservationsCount { get; set; }
        public long? TaxaCount { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Zoom { get; set; }
        public LatLonBoundingBox BoundingBox { get; set; }
        public static GridCellTile Create(string key, long? observationsCount, long? taxaCount)
        {
            var zoomAndCoordinates = GeoTileHelper.GetZoomAndCoordinatesFromKey(key);
            GridCellTile gridCellTile = new GridCellTile
            {
                Zoom = zoomAndCoordinates.zoom, 
                X = zoomAndCoordinates.x, 
                Y = zoomAndCoordinates.y,
                BoundingBox = new LatLonBoundingBox
                {
                    TopLeft = GeoTileHelper.GetCoordinateFromTile(zoomAndCoordinates.x, zoomAndCoordinates.y, zoomAndCoordinates.zoom),
                    BottomRight = GeoTileHelper.GetCoordinateFromTile(zoomAndCoordinates.x+1, zoomAndCoordinates.y+1, zoomAndCoordinates.zoom),
                },
                ObservationsCount = observationsCount,
                TaxaCount = taxaCount,
            };

            return gridCellTile;
        }

        public Feature GetFeature(CoordinateSys coordinateSystem)
        {
            var attributes = new AttributesTable
            {
                {"ObservationsCount", ObservationsCount},
                {"TaxaCount", TaxaCount},
                {"X", X},
                {"Y", Y},
                {"Zoom", Zoom}
            };

            var feature = new Feature(BoundingBox.GetPolygon(coordinateSystem), attributes);
            return feature;
        }
    }
}