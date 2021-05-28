﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Helpers
{
    public static class GeoJsonHelper
    {
        public enum GeoJsonGeometryType
        {
            Point = 0,
            PointWithBuffer = 1,
            PointWithDisturbanceBuffer = 2,
        }

        private static readonly NetTopologySuite.IO.GeoJsonReader GeoJsonReader = new NetTopologySuite.IO.GeoJsonReader();

        public static string GetFeatureCollectionString(IEnumerable<IDictionary<string, object>> records, bool flattenProperties)
        {
            var featureCollection = GetFeatureCollection(records, flattenProperties);
            var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
            var strJson = geoJsonWriter.Write(featureCollection);
            return strJson;
        }

        public static FeatureCollection GetFeatureCollection(IEnumerable<IDictionary<string, object>> records, bool flattenProperties)
        {
            var featureCollection = new FeatureCollection();

            foreach (var observation in records)
            {
                var feature = GetFeature(observation, flattenProperties, GeoJsonGeometryType.Point);
                featureCollection.Add(feature);
            }

            return featureCollection;
        }

        public static Feature GetFeature(IDictionary<string, object> record, bool flattenProperties, GeoJsonGeometryType geometryType = GeoJsonGeometryType.Point)
        {
            Geometry geometry = null;
            var attributesDictionary = flattenProperties ? FlattenDictionary(record) : record;
            
            if (record.TryGetValue(nameof(Observation.Location).ToLower(),
                out var locationObject))
            {
                var locationDictionary = locationObject as IDictionary<string, object>;
                if (geometryType == GeoJsonGeometryType.Point)
                {
                    var decimalLatitude = (double)locationDictionary["decimalLatitude"];
                    var decimalLongitude = (double)locationDictionary["decimalLongitude"];
                    geometry = new Point(decimalLongitude, decimalLatitude);
                }
                else if (geometryType == GeoJsonGeometryType.PointWithBuffer)
                {
                    var str = JsonSerializer.Serialize(locationDictionary["pointWithBuffer"]);
                    geometry = GeoJsonReader.Read<Polygon>(str);
                }
                else if (geometryType == GeoJsonGeometryType.PointWithDisturbanceBuffer)
                {
                    var str = JsonSerializer.Serialize(locationDictionary["pointWithDisturbanceBuffer"]);
                    geometry = GeoJsonReader.Read<Polygon>(str);
                }
            }

            var feature = new Feature(geometry, new AttributesTable(attributesDictionary));
            return feature;
        }

        private static IDictionary<string, object> FlattenDictionary(IDictionary<string, object> dictionary)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            FlattenDictionary("", dictionary, result);
            return result;
        }

        private static void FlattenDictionary(
            string prefix,
            IDictionary<string, object> sourceDictionary,
            IDictionary<string, object> resultDictionary)
        {
            foreach (var pair in sourceDictionary)
            {
                if (pair.Value is IDictionary<string, object> subDictionary)
                {
                    FlattenDictionary(prefix + pair.Key + ".", subDictionary, resultDictionary);
                }
                else if (pair.Value is IList<object> list)
                {
                    bool isChildrenDictionaries = list.OfType<IDictionary<string, object>>().Any();
                    if (isChildrenDictionaries)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] is IDictionary<string, object> subListDictionary)
                            {
                                FlattenDictionary($"{prefix}{pair.Key}[{i}].", subListDictionary, resultDictionary);
                            }
                        }
                    }
                    else
                    {
                        resultDictionary.Add(prefix + pair.Key, pair.Value);
                    }
                }
                else
                {
                    resultDictionary.Add(prefix + pair.Key, pair.Value);
                }
            }
        }
    }
}