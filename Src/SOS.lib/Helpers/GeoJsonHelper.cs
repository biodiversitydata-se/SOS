﻿using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

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
        private static readonly NetTopologySuite.IO.GeoJsonWriter GeoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();

        /// <summary>
        /// Add missing grid cells to grid
        /// </summary>
        /// <param name="metricGridCellFeatures"></param>
        /// <param name="metricEooGeometries"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="attributes"></param>
        /// <param name="useCenterPoint"></param>
        public static void FillInBlanks(
            IDictionary<string, IFeature> metricGridCellFeatures,
            IDictionary<double, Geometry> metricEooGeometries,
            int gridCellsInMeters,
            IEnumerable<KeyValuePair<string, object>> attributes,
            bool useCenterPoint)
        {
            var emptyGridCellFeaturesSweRef99 = new Dictionary<string, IFeature>();

            // Start at top left gridcell bottom left corner

            var minX = metricGridCellFeatures.Min(gc => gc.Value.Geometry.Coordinates.Min(c => c.X));
            var maxX = metricGridCellFeatures.Max(gc => gc.Value.Geometry.Coordinates.Max(c => c.X));
            var minY = metricGridCellFeatures.Min(gc => gc.Value.Geometry.Coordinates.Min(c => c.Y));
            var x = minX;
            var y = metricGridCellFeatures.Max(gc => gc.Value.Geometry.Coordinates.Max(c => c.Y)) - gridCellsInMeters;
            while (y >= minY)
            {
                while (x < maxX)
                {
                    var id = GeoJsonHelper.GetGridCellId(gridCellsInMeters, (int)x, (int)y);

                    // Try to get grid cell
                    if (!metricGridCellFeatures.TryGetValue(id, out var feature))
                    {
                        // Grid cell is missing, create a new one
                        feature = new Feature(
                            new Polygon(
                                new LinearRing(
                                    new[] {
                                        new Coordinate(x, y), // bottom left
                                        new Coordinate(x, y + gridCellsInMeters), // top left 
                                        new Coordinate(x + gridCellsInMeters, y + gridCellsInMeters), // top rigth
                                        new Coordinate(x + gridCellsInMeters, y), // bottom rigth
                                        new Coordinate(x, y) // bottom left
                                    }
                            )),
                            new AttributesTable(
                                new KeyValuePair<string, object>[] {
                                            new KeyValuePair<string, object>("id", id)
                                }.Concat(attributes)
                            )
                        );

                        var emptyCellAdded = false;
                        foreach (var metricEoo in metricEooGeometries)
                        {
                            if (emptyCellAdded)
                            {
                                continue;
                            }

                            var alphaValue = metricEoo.Key;
                            var eooGeometry = metricEoo.Value;
                            var emptyCell = feature.Geometry;
                            if (!eooGeometry.Contains(emptyCell))
                            {
                                continue;
                            }

                            foreach (var gridCellFeature in metricGridCellFeatures)
                            {
                                var gridCell = gridCellFeature.Value.Geometry;
                                if (useCenterPoint)
                                {
                                    if (gridCell.Centroid.Distance(emptyCell.Centroid) <= alphaValue)
                                    {
                                        emptyGridCellFeaturesSweRef99.TryAdd(id, feature);
                                        emptyCellAdded = true;
                                    }
                                    continue;
                                }

                                if (gridCell.Distance(emptyCell) <= alphaValue)
                                {
                                    emptyGridCellFeaturesSweRef99.TryAdd(id, feature);
                                    emptyCellAdded = true;
                                }
                            }
                        }
                    }

                    x += gridCellsInMeters;
                }

                x = minX;
                y -= gridCellsInMeters;
            }

            // Add empty grid cells to result set
            foreach (var emptyGridCellFeature in emptyGridCellFeaturesSweRef99)
            {
                metricGridCellFeatures.Add(emptyGridCellFeature);
            }
        }

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
                var feature = GetFeature(observation, flattenProperties);
                featureCollection.Add(feature);
            }

            return featureCollection;
        }

        public static Feature GetFeature(IDictionary<string, object> record, bool flattenProperties, GeoJsonGeometryType geometryType = GeoJsonGeometryType.Point)
        {
            Geometry geometry = null;

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
                    locationDictionary.Remove("pointWithBuffer"); // Remove from properties. Just need this for geometry.
                }
                else if (geometryType == GeoJsonGeometryType.PointWithDisturbanceBuffer)
                {
                    var str = JsonSerializer.Serialize(locationDictionary["pointWithDisturbanceBuffer"]);
                    geometry = GeoJsonReader.Read<Polygon>(str);
                    locationDictionary.Remove("pointWithDisturbanceBuffer"); // Remove from properties. Just need this for geometry.
                }
            }

            var attributesDictionary = flattenProperties ? FlattenDictionary(record) : record;
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

        public static Feature GetFeature(IGeoShape geometry, AttributesTable attributesTable)
        {
            var geom = geometry.ToGeometry();
            var feature = new Feature(geom, attributesTable);
            return feature;
        }

        public static string GetFeatureAsGeoJsonString(IGeoShape geometry, AttributesTable attributesTable)
        {
            var feature = GetFeature(geometry, attributesTable);
            string strGeoJson = GeoJsonWriter.Write(feature);
            return strGeoJson;
        }

        public static string GetGridCellId(int gridCellSizeInMeters, int left, int bottom)
        {
            var gridSizeInKm = gridCellSizeInMeters / 1000.0;
            var gridSizesInM = gridCellSizeInMeters.ToString();
            var noOfZeros = gridSizesInM.Length - gridSizesInM.TrimEnd('0').Length;
            string lowLeftX = left.ToString();
            string lowLeftY = bottom.ToString();
            var strLowerLeftY = lowLeftY.Remove(lowLeftY.ToString().Length - noOfZeros, noOfZeros);
            var strLowerLeftX = lowLeftX.Remove(lowLeftX.ToString().Length - noOfZeros, noOfZeros);
            string id;
            if (gridCellSizeInMeters < 1000)
                id = $"{gridCellSizeInMeters}mN{strLowerLeftY}E{strLowerLeftX}";
            else
                id = $"{gridSizeInKm.ToString(CultureInfo.InvariantCulture)}kmN{strLowerLeftY}E{strLowerLeftX}";
            return id;
        }
    }
}
