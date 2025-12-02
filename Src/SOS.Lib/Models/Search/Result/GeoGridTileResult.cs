using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SOS.Lib.Enums;
using SOS.Lib.Models.Gis;
using System.Collections.Generic;

namespace SOS.Lib.Models.Search.Result;

public class GeoGridTileResult
{
    public LatLonBoundingBox BoundingBox { get; set; }
    public int Zoom { get; set; }
    public int GridCellTileCount { get; set; }
    public IEnumerable<GridCellTile> GridCellTiles { get; set; }

    public FeatureCollection GetFeatureCollection(CoordinateSys coordinateSystem = CoordinateSys.WGS84, LatLonBoundingBox? bbox = null)
    {
        var featureCollection = new FeatureCollection();
        if (bbox != null)
        {
            featureCollection.BoundingBox = bbox.ToEnvelope();
        }

        foreach (var gridCellTile in GridCellTiles)
        {
            var feature = gridCellTile.GetFeature(coordinateSystem);
            featureCollection.Add(feature);
        }

        return featureCollection;
    }

    public string GetFeatureCollectionGeoJson(CoordinateSys coordinateSystem = CoordinateSys.WGS84, LatLonBoundingBox? bbox = null)
    {
        var featureCollection = GetFeatureCollection(coordinateSystem, bbox);
        var geoJsonWriter = new GeoJsonWriter();
        var strJson = geoJsonWriter.Write(featureCollection);
        return strJson;
    }
}