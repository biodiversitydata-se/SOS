using System.Collections.Generic;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Models.Gis
{
    public class GeoGridTileTaxaCell
    {
        public LatLonBoundingBox BoundingBox { get; set; }
        public string GeoTile { get; set; }
        public int Zoom { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public IEnumerable<GeoGridTileTaxonObservationCount> Taxa { get; set; }

        public static GeoGridTileTaxaCell Create(string key, List<GeoGridTileTaxonObservationCount> taxaCounts)
        {
            var zoomAndCoordinates = GeoTileHelper.GetZoomAndCoordinatesFromKey(key);
            var gridCellTile = new GeoGridTileTaxaCell
            {
                GeoTile = key,
                Zoom = zoomAndCoordinates.zoom,
                X = zoomAndCoordinates.x,
                Y = zoomAndCoordinates.y,
                BoundingBox = new LatLonBoundingBox
                {
                    TopLeft = GeoTileHelper.GetCoordinateFromTile(zoomAndCoordinates.x, zoomAndCoordinates.y, zoomAndCoordinates.zoom),
                    BottomRight = GeoTileHelper.GetCoordinateFromTile(zoomAndCoordinates.x + 1, zoomAndCoordinates.y + 1, zoomAndCoordinates.zoom),
                },
                Taxa = taxaCounts
            };

            return gridCellTile;
        }
    }
}