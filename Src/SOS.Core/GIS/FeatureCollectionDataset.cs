using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;

namespace SOS.Core.GIS
{    
    public class FeatureCollectionDataset
    {
        private readonly STRtree<IFeature> _strTree;        
        public FeatureCollection FeatureCollection { get; }

        public FeatureCollectionDataset(FeatureCollection featureCollection)
        {
            FeatureCollection = featureCollection;
            _strTree = CreateSTRTree(featureCollection);
        }

        public static FeatureCollectionDataset CreateFromGeoJsonFile(string filePath)
        {            
            var featureCollection = LoadFeatureCollection(filePath);            
            return new FeatureCollectionDataset(featureCollection);
        }

        private static FeatureCollection LoadFeatureCollection(string filePath)
        {            
            var geoJsonReader = new GeoJsonReader();            
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var featureCollection = geoJsonReader.Read<FeatureCollection>(str);            
            return featureCollection;
        }

        private STRtree<IFeature> CreateSTRTree(FeatureCollection featureCollection)
        {
            STRtree<IFeature> strTree = new STRtree<IFeature>();            
            foreach (IFeature feature in featureCollection.Features)
            {                    
                strTree.Insert(feature.Geometry.EnvelopeInternal, feature);                    
            }    
            
            strTree.Build();
            return strTree;
        }

        public IFeature FindFirstFeatureContainingPoint(IPoint point)
        {                                       
            var possibleFeatures = _strTree.Query(point.EnvelopeInternal);
            foreach (IFeature feature in possibleFeatures)
            {
                if (feature.Geometry.Contains(point))
                {
                    return feature;
                }
            }

            return null;
        }

        public List<IFeature> FindFeaturesContainingPoint(IPoint point)
        {                                       
            List<IFeature> featuresContainingPoint = new List<IFeature>();
            var possibleFeatures = _strTree.Query(point.EnvelopeInternal);
            foreach (IFeature feature in possibleFeatures)
            {
                if (feature.Geometry.Contains(point))
                {
                    featuresContainingPoint.Add(feature);                    
                }
            }

            return featuresContainingPoint;
        }
    }
}
