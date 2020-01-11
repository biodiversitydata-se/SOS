using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace SOS.Core.GIS
{
    // Todo - Add support for other coordinate systems.
    public class FeatureDataset
    {                        
        private readonly IFeature _feature;
        public IFeature Feature => _feature;

        public FeatureDataset(IFeature feature)
        {
            _feature = feature;
        }

        public static FeatureDataset CreateFromGeoJsonFile(string filePath)
        {
            var featureCollection = LoadFeatureCollection(filePath);
            if (featureCollection?.Features == null)
            {
                throw new Exception("The feature collection must contain exactly 1 feature. This feature collection contains 0 features.");
            }
            if (featureCollection.Features.Count != 1)
            {
                throw new Exception($"The feature collection must contain exactly 1 feature. This feature collection contains {featureCollection.Features.Count} features.");
            }

            return new FeatureDataset(featureCollection.Features[0]);
        }

        private static FeatureCollection LoadFeatureCollection(string filePath)
        {            
            var geoJsonReader = new GeoJsonReader();            
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var featureCollection = geoJsonReader.Read<FeatureCollection>(str);            
            return featureCollection;
        }
        
        public bool IsInsideGeometry(IPoint point)
        {
            return _feature.Geometry.Contains(point);            
        }
    }
}