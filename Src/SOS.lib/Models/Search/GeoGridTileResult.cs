using System.Collections.Generic;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;

namespace SOS.Lib.Models.Search
{
    public class GeoGridTileResult
    {
        public LatLonBoundingBox BoundingBox { get; set; }
        public int Zoom { get; set; }
        public int GridCellTileCount { get; set; }
        public IEnumerable<GridCellTile> GridCellTiles { get; set; }

        public FeatureCollection GetFeatureCollection(CoordinateSys coordinateSystem = CoordinateSys.WGS84)
        {
            var featureCollection = new FeatureCollection();
            
            foreach (var gridCellTile in GridCellTiles)
            {
                var feature = gridCellTile.GetFeature(coordinateSystem);
                featureCollection.Add(feature);
            }

            return featureCollection;
        }

        public string GetFeatureCollectionGeoJson(CoordinateSys coordinateSystem = CoordinateSys.WGS84)
        {
            var featureCollection = GetFeatureCollection(coordinateSystem);
            var geoJsonWriter = new GeoJsonWriter();
            var strJson = geoJsonWriter.Write(featureCollection);
            return strJson;
        }
    }
}