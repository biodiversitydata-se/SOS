using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Core.Models.Observations;

namespace SOS.Core.GIS
{
    public static class FileBasedGeographyService
    {
        private static FeatureCollectionsDataset RegionsGeographyDataset => _regionsGeographyDataset.Value;
        private static FeatureCollectionDataset SwedenGeographyDataset => _swedenGeographyDataset.Value;

        private static readonly Lazy<FeatureCollectionsDataset> _regionsGeographyDataset = new Lazy<FeatureCollectionsDataset>(() =>
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] filePaths =
            {
                Path.Combine(assemblyPath, @"Resources\GIS\Counties.geojson"), // Län
                Path.Combine(assemblyPath, @"Resources\GIS\Municipalities.geojson"), // Kommun                            
                Path.Combine(assemblyPath, @"Resources\GIS\Provinces.geojson"), // Landskap
                Path.Combine(assemblyPath, @"Resources\GIS\CountryParts.geojson") // Landsdel
                //Path.Combine(assemblyPath, @"Resources\GIS\Congregations.geojson"), // Församling                
                //Path.Combine(assemblyPath, @"Resources\GIS\Parishes.geojson"), // Socken                            
            };

            return FeatureCollectionsDataset.CreateFromGeoJsonFiles(filePaths);
        }, true);

        private static readonly Lazy<FeatureCollectionDataset> _swedenGeographyDataset = new Lazy<FeatureCollectionDataset>(() =>
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return FeatureCollectionDataset.CreateFromGeoJsonFile(Path.Combine(assemblyPath, @"Resources\GIS\SimplifiedSwedenExtents.geojson"));
        }, true);


        public static bool IsObservationInSweden(ProcessedDwcObservation observation)
        {
            IPoint webMercatorPoint = new Point(observation.CoordinateX_WebMercator, observation.CoordinateY_WebMercator);
            //string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //FeatureCollectionDataset swedenGeographyDataset = FeatureCollectionDataset.CreateFromGeoJsonFile(Path.Combine(assemblyPath, @"Resources\GIS\SimplifiedSwedenExtents.geojson"));
            var features = SwedenGeographyDataset.FindFirstFeatureContainingPoint(webMercatorPoint);
            return features != null;
        }

        public static void CalculateRegionBelongings(ProcessedDwcObservation observation)
        {
            try
            {
                IPoint webMercatorPoint = new Point(observation.CoordinateX_WebMercator, observation.CoordinateY_WebMercator);
                var foundFeatures = RegionsGeographyDataset.FindFeaturesContainingPoint(webMercatorPoint);
                var dic = GetDictionaryFromAttributesTable(foundFeatures);
                // todo - add enums for Kommun, Landskap, ...            

                if (dic.ContainsKey("Kommun"))
                {
                    observation.MunicipalityIdByCoordinate = int.Parse(dic["Kommun"]["nativeId"].ToString());
                    observation.DebugMunicipalityNameByCoordinate = dic["Kommun"]["name"].ToString();
                }

                if (dic.ContainsKey("Landskap"))
                {
                    observation.ProvinceIdByCoordinate = int.Parse(dic["Landskap"]["nativeId"].ToString());
                    observation.DebugProvinceNameByCoordinate = dic["Landskap"]["name"].ToString();
                }

                if (dic.ContainsKey("Län"))
                {
                    observation.CountyIdByCoordinate = int.Parse(dic["Län"]["nativeId"].ToString());
                    observation.DebugCountyNameByCoordinate = dic["Län"]["name"].ToString();
                }

                if (dic.ContainsKey("Landsdel"))
                {
                    observation.CountryPartIdByCoordinate = int.Parse(dic["Landsdel"]["nativeId"].ToString());
                    observation.DebugCountryPartNameByCoordinate = dic["Landsdel"]["name"].ToString();
                }

                // todo - add CountyPartIdByCoordinate
            }
            catch { }
        }

        private static Dictionary<string, IAttributesTable> GetDictionaryFromAttributesTable(List<IFeature> features)
        {
            Dictionary<string, IAttributesTable> dic = new Dictionary<string, IAttributesTable>();
            foreach (var feature in features)
            {
                dic.Add(feature.Attributes["categoryName"].ToString(), feature.Attributes);
            }

            return dic;
        }
    }

    public class FileBasedGeographyServiceEx : IGeographyService
    {
        private FeatureCollectionsDataset RegionsGeographyDataset => _regionsGeographyDataset.Value;
        private FeatureCollectionDataset SwedenGeographyDataset => _swedenGeographyDataset.Value;

        private readonly Lazy<FeatureCollectionsDataset> _regionsGeographyDataset = new Lazy<FeatureCollectionsDataset>(() =>
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string[] filePaths =
            {
                Path.Combine(assemblyPath, @"Resources\GIS\Counties.geojson"), // Län
                Path.Combine(assemblyPath, @"Resources\GIS\Municipalities.geojson"), // Kommun                            
                Path.Combine(assemblyPath, @"Resources\GIS\Provinces.geojson"), // Landskap
                Path.Combine(assemblyPath, @"Resources\GIS\CountryParts.geojson") // Landsdel
                //Path.Combine(assemblyPath, @"Resources\GIS\Congregations.geojson"), // Församling                
                //Path.Combine(assemblyPath, @"Resources\GIS\Parishes.geojson"), // Socken                            
            };

            return FeatureCollectionsDataset.CreateFromGeoJsonFiles(filePaths);
        }, true);

        private readonly Lazy<FeatureCollectionDataset> _swedenGeographyDataset = new Lazy<FeatureCollectionDataset>(() =>
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return FeatureCollectionDataset.CreateFromGeoJsonFile(Path.Combine(assemblyPath, @"Resources\GIS\SimplifiedSwedenExtents.geojson"));
        }, true);


        public bool IsObservationInSweden(ProcessedDwcObservation observation)
        {
            IPoint webMercatorPoint = new Point(observation.CoordinateX_WebMercator, observation.CoordinateY_WebMercator);
            //string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //FeatureCollectionDataset swedenGeographyDataset = FeatureCollectionDataset.CreateFromGeoJsonFile(Path.Combine(assemblyPath, @"Resources\GIS\SimplifiedSwedenExtents.geojson"));
            var features = SwedenGeographyDataset.FindFirstFeatureContainingPoint(webMercatorPoint);
            return features != null;
        }

        public void CalculateRegionBelongings(ProcessedDwcObservation observation)
        {
            try
            {
                IPoint webMercatorPoint = new Point(observation.CoordinateX_WebMercator, observation.CoordinateY_WebMercator);
                var foundFeatures = RegionsGeographyDataset.FindFeaturesContainingPoint(webMercatorPoint);
                var dic = GetDictionaryFromAttributesTable(foundFeatures);
                // todo - add enums for Kommun, Landskap, ...            

                if (dic.ContainsKey("Kommun"))
                {
                    observation.MunicipalityIdByCoordinate = int.Parse(dic["Kommun"]["nativeId"].ToString());
                    observation.DebugMunicipalityNameByCoordinate = dic["Kommun"]["name"].ToString();
                }

                if (dic.ContainsKey("Landskap"))
                {
                    observation.ProvinceIdByCoordinate = int.Parse(dic["Landskap"]["nativeId"].ToString());
                    observation.DebugProvinceNameByCoordinate = dic["Landskap"]["name"].ToString();
                }

                if (dic.ContainsKey("Län"))
                {
                    observation.CountyIdByCoordinate = int.Parse(dic["Län"]["nativeId"].ToString());
                    observation.DebugCountyNameByCoordinate = dic["Län"]["name"].ToString();
                }

                if (dic.ContainsKey("Landsdel"))
                {
                    observation.CountryPartIdByCoordinate = int.Parse(dic["Landsdel"]["nativeId"].ToString());
                    observation.DebugCountryPartNameByCoordinate = dic["Landsdel"]["name"].ToString();
                }

                // todo - add CountyPartIdByCoordinate
            }
            catch { }
        }

        private static Dictionary<string, IAttributesTable> GetDictionaryFromAttributesTable(List<IFeature> features)
        {
            Dictionary<string, IAttributesTable> dic = new Dictionary<string, IAttributesTable>();
            foreach (var feature in features)
            {
                dic.Add(feature.Attributes["categoryName"].ToString(), feature.Attributes);
            }

            return dic;
        }
    }

}
