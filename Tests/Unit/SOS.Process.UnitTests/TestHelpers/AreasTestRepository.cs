using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Shared;

namespace SOS.Process.UnitTests.TestHelpers
{
    public static class AreasTestRepository
    {
        public static IEnumerable<AreaWithGeometry> LoadAreas(IEnumerable<AreaType> areaTypes)
        {
            var areaCollection = new List<AreaWithGeometry>();
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (var areaType in areaTypes)
            {
                IEnumerable<AreaWithGeometry> areas;
                switch (areaType)
                {
                    case AreaType.County:
                        areas = LoadAreas(Path.Combine(assemblyPath, @"Resources/Regions/Counties.geojson"), areaType);
                        areaCollection.AddRange(areas);
                        break;
                    case AreaType.Province:
                        areas = LoadAreas(Path.Combine(assemblyPath, @"Resources/Regions/Provinces.geojson"), areaType);
                        areaCollection.AddRange(areas);
                        break;
                    default:
                        throw new ArgumentException($"Not handled AreaType: {areaType}");
                }
            }

            return areaCollection;
        }

        private static IEnumerable<AreaWithGeometry> LoadAreas(string filePath, AreaType areaType)
        {
            var featureCollection = LoadFeatureCollection(filePath);
            var areas = ConvertToAreas(featureCollection, areaType);
            return areas;
        }

        private static FeatureCollection LoadFeatureCollection(string filePath)
        {
            var geoJsonReader = new GeoJsonReader();
            var str = File.ReadAllText(filePath, Encoding.UTF8);
            var featureCollection = geoJsonReader.Read<FeatureCollection>(str);
            return featureCollection;
        }

        private static IEnumerable<AreaWithGeometry> ConvertToAreas(IEnumerable<IFeature> features, AreaType areaType)
        {
            return features.Select(a => ConvertToArea(a, areaType));
        }

        private static AreaWithGeometry ConvertToArea(IFeature feature, AreaType areaType)
        {
            return new AreaWithGeometry(areaType, feature.Attributes["nativeId"].ToString())
            {
                Geometry = feature.Geometry.Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84),
                Name = feature.Attributes["name"].ToString()
            };
        }
    }
}