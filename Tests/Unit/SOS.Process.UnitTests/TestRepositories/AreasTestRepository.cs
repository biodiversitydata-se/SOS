using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Test.TestRepositories
{
    public static class AreasTestRepository
    {
        public static IEnumerable<Area> LoadAreas(IEnumerable<AreaType> areaTypes)
        {
            List<Area> areaCollection = new List<Area>();
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var areaType in areaTypes)
            {
                IEnumerable<Area> areas;
                switch (areaType)
                {
                    case AreaType.County:
                        areas = LoadAreas(Path.Combine(assemblyPath, @"Resources\Regions\Counties.geojson"), areaType);
                        areaCollection.AddRange(areas);
                        break;
                    case AreaType.Province:
                        areas = LoadAreas(Path.Combine(assemblyPath, @"Resources\Regions\Provinces.geojson"), areaType);
                        areaCollection.AddRange(areas);
                        break;
                    default:
                        throw new ArgumentException($"Not handled AreaType: {areaType}");
                }
            }

            return areaCollection;
        }

        private static IEnumerable<Area> LoadAreas(string filePath, AreaType areaType)
        {
            FeatureCollection featureCollection = LoadFeatureCollection(filePath);
            var areas = ConvertToAreas(featureCollection, areaType);
            return areas;
        }

        private static FeatureCollection LoadFeatureCollection(string filePath)
        {
            var geoJsonReader = new GeoJsonReader();
            string str = File.ReadAllText(filePath, Encoding.UTF8);
            var featureCollection = geoJsonReader.Read<FeatureCollection>(str);
            return featureCollection;
        }

        private static IEnumerable<Area> ConvertToAreas(IEnumerable<IFeature> features, AreaType areaType)
        {
            return features.Select(a => ConvertToArea(a, areaType));
        }

        private static Area ConvertToArea(IFeature feature, AreaType areaType)
        {
            return new Area(areaType)
            {
                Id = int.Parse(feature.Attributes["id"].ToString()),
                FeatureId = int.Parse(feature.Attributes["nativeId"].ToString()),
                //ParentId = feature.ParentId,
                Geometry = feature.Geometry.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84).ToGeoJsonGeometry(),
                Name = feature.Attributes["name"].ToString()
            };
        }
    }
}
