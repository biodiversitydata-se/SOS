using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;

namespace SOS.Core.GIS
{
    public class FeatureCollectionsDataset
    {
        private readonly STRtree<IFeature> _strTree;
        public List<FeatureCollection> FeatureCollections { get; }

        public FeatureCollectionsDataset(List<FeatureCollection> featureCollections)
        {
            FeatureCollections = featureCollections;                        
            _strTree = CreateSTRTree(featureCollections);            
        }

        public static FeatureCollectionsDataset CreateFromGeoJsonFiles(string[] filePaths)
        {
            List<FeatureCollection> featureCollections = null;
            int NumberOfRetries = 10;
            int MinDelayOnRetry = 1000;
            int MaxDelayOnRetry = 5000;
            Random rnd = new Random();
            for (int i=1; i <= NumberOfRetries; ++i) 
            {
                try 
                {
                    featureCollections = LoadFeatureCollections(filePaths);
                    break;
                }
                catch (IOException) when (i <= NumberOfRetries) 
                {                                        
                    Thread.Sleep(rnd.Next(MinDelayOnRetry, MaxDelayOnRetry));
                }
            }

            if (featureCollections == null)
            {
                throw new IOException($"Couldn't load the geography region files: \"{ string.Join(",", filePaths) }\"");
            }

            return new FeatureCollectionsDataset(featureCollections);            
        }

        private static List<FeatureCollection> LoadFeatureCollections(string[] filePaths)
        {
            List<FeatureCollection> featureCollections = new List<FeatureCollection>(filePaths.Length);
            var geoJsonReader = new GeoJsonReader();
            foreach (string filePath in filePaths)
            {
                string strWkt = File.ReadAllText(filePath, Encoding.UTF8);
                var featureCollection = geoJsonReader.Read<FeatureCollection>(strWkt);
                featureCollections.Add(featureCollection);
            }

            return featureCollections;
        }

        private STRtree<IFeature> CreateSTRTree(List<FeatureCollection> featureCollections)
        {
            STRtree<IFeature> strTree = new STRtree<IFeature>();
            foreach (FeatureCollection featureCollection in featureCollections)
            {
                foreach (IFeature feature in featureCollection.Features)
                {                    
                    strTree.Insert(feature.Geometry.EnvelopeInternal, feature);                    
                }    
            }
            
            strTree.Build();
            return strTree;
        }

        public List<IFeature> FindFeaturesContainingPoint(IPoint point)
        {               
            List<IFeature> featuresContainingPoint = new List<IFeature>(FeatureCollections.Count);
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
