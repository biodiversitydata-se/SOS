using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SOS.Lib.Helpers;

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

    /// <summary>
    /// Add missing grid cells to grid
    /// </summary>
    /// <param name="metricGridCellFeatures"></param>
    /// <param name="gridCellsInMeters"></param>
    /// <param name="attributes"></param>
    public static void FillInBlanks(
        IDictionary<string, IFeature> metricGridCellFeatures,
        int gridCellsInMeters,
        IEnumerable<KeyValuePair<string, object>> attributes)
    {
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
                var id = GetGridCellId(gridCellsInMeters, (int)x, (int)y);
                
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

                    metricGridCellFeatures.Add(id, feature);
                }

                x += gridCellsInMeters;
            }

            x = minX;
            y -= gridCellsInMeters;
        }
    }

    public static string GetFeatureCollectionString(IEnumerable<JsonObject> records, bool flattenProperties)
    {
        var featureCollection = GetFeatureCollection(records, flattenProperties);
        var geoJsonWriter = new NetTopologySuite.IO.GeoJsonWriter();
        var strJson = geoJsonWriter.Write(featureCollection);
        return strJson;
    }

    public static FeatureCollection GetFeatureCollection(IEnumerable<JsonObject> records, bool flattenProperties)
    {
        var featureCollection = new FeatureCollection();

        foreach (var observation in records)
        {
            var feature = GetFeature(observation, flattenProperties);
            featureCollection.Add(feature);
        }

        return featureCollection;
    }        

    public static Feature GetFeature(JsonNode record, bool flattenProperties, GeoJsonGeometryType geometryType = GeoJsonGeometryType.Point)
    {
        Geometry geometry = null;
                    
        if (record is JsonObject obj &&
            obj.TryGetPropertyValue(nameof(Observation.Location).ToLower(), out JsonNode locationNode))
        {
            if (geometryType == GeoJsonGeometryType.Point)
            {               
                var decimalLatitude = locationNode["decimalLatitude"].GetValue<double>();
                var decimalLongitude = locationNode["decimalLongitude"].GetValue<double>();
                geometry = new Point(decimalLongitude, decimalLatitude);
            }
            else if (geometryType == GeoJsonGeometryType.PointWithBuffer)
            {                    
                var str = JsonSerializer.Serialize(locationNode["pointWithBuffer"]);
                //var str = locationNode["pointWithBuffer"].ToJsonString();
                ((JsonObject)locationNode).Remove("pointWithBuffer"); // Remove from properties. Just need this for geometry.                    
            }
            else if (geometryType == GeoJsonGeometryType.PointWithDisturbanceBuffer)
            {
                var str = JsonSerializer.Serialize(locationNode["pointWithDisturbanceBuffer"]);
                //var str = locationNode["pointWithDisturbanceBuffer"].ToJsonString();
                ((JsonObject)locationNode).Remove("pointWithDisturbanceBuffer"); // Remove from properties. Just need this for geometry.    
            }
        }

        var attributesDictionary = flattenProperties ? CreateFlatDictionary(record) : CreateHierarchicalDictionary(record);
        var feature = new Feature(geometry, new AttributesTable(attributesDictionary));
        return feature;
    }        

    private static IDictionary<string, object> CreateHierarchicalDictionary(JsonNode record)
    {            
        var result = ToHierarchicalDictionary(record) as Dictionary<string, object?>;
        return result;
    }

    private static object? ToHierarchicalDictionary(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                var dict = new Dictionary<string, object?>();
                foreach (var kvp in obj)
                {
                    dict[kvp.Key] = ToHierarchicalDictionary(kvp.Value);
                }
                return dict;

            case JsonArray array:
                return array.Select(ToHierarchicalDictionary).ToList();

            case JsonValue value:
                return ExtractValue(value);

            case null:
                return null;

            default:
                return node.ToJsonString();
        }
    }

    private static IDictionary<string, object> CreateFlatDictionary(JsonNode record)
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        PopulateFlatDictionary("", record, result);
        return result;
    }

    private static void PopulateFlatDictionary(
        string prefix,
        JsonNode node,
        IDictionary<string, object?> resultDictionary)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var kvp in obj)
                {
                    PopulateFlatDictionary($"{prefix}{kvp.Key}.", kvp.Value, resultDictionary);
                }
                break;

            case JsonArray array:
                for (int i = 0; i < array.Count; i++)
                {
                    PopulateFlatDictionary($"{prefix}[{i}].", array[i], resultDictionary);
                }
                break;

            case JsonValue value:
                resultDictionary[prefix.TrimEnd('.')] = value.ToJsonString();                    
                object? actualValue = value.TryGetValue<JsonElement>(out var je)
                    ? ExtractJsonElementValue(je)
                    : value.GetValue<object?>();
                resultDictionary[prefix.TrimEnd('.')] = actualValue;                    
                break;

            case null:
                resultDictionary[prefix.TrimEnd('.')] = null;
                break;
        }
    }

    private static object? ExtractJsonElementValue(JsonElement je)
    {
        if (je.ValueKind == JsonValueKind.Number)
        {
            if (je.TryGetInt32(out var i)) return i;
            if (je.TryGetInt64(out var l)) return l;                
            return je.GetDouble();
        }
        
        return je.ValueKind switch
        {
            JsonValueKind.String => je.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => je.GetValue(false)
        };
    }

    private static object? ExtractValue(JsonValue value)
    {
        if (value.TryGetValue<JsonElement>(out var je))
        {
            if (je.ValueKind == JsonValueKind.Number)
            {
                if (je.TryGetInt32(out var i)) return i;
                if (je.TryGetInt64(out var l)) return l;
                return je.GetDouble();
            }

            return je.ValueKind switch
            {
                JsonValueKind.String => je.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => je.ToString()
            };                
        }

        return value.GetValue<object?>();
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
