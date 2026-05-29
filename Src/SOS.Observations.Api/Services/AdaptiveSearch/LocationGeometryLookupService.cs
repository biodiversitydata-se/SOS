using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Observations.Api.Managers.Interfaces;
using System.Text.Json.Nodes;
using ProcessedLocationType = SOS.Lib.Enums.LocationType;

namespace SOS.Observations.Api.Services.AdaptiveSearch;

public interface ILocationGeometryLookupService
{
    Task<Dictionary<string, Geometry>?> GetPolygonLocationGeometriesAsync(IEnumerable<JsonObject> records);
    Task<Dictionary<string, Geometry>?> GetPolygonLocationGeometriesAsync(IEnumerable<(ProcessedLocationType? Type, IEnumerable<string> LocationIds)> locations);
}

internal sealed class LocationGeometryLookupService : ILocationGeometryLookupService
{
    private readonly ILocationManager _locationManager;

    public LocationGeometryLookupService(ILocationManager locationManager)
    {
        _locationManager = locationManager ?? throw new ArgumentNullException(nameof(locationManager));
    }

    public async Task<Dictionary<string, Geometry>?> GetPolygonLocationGeometriesAsync(IEnumerable<JsonObject> records)
    {
        var polygonLocationIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var record in records)
        {
            if (record.TryGetPropertyValue("location", out var locationNode) && locationNode is JsonObject locationObj)
            {
                var locationTypeInt = locationObj["type"]?.GetValue<int>();
                var locationType = (ProcessedLocationType?)locationTypeInt;
                var locationId = locationObj["locationId"]?.GetValue<string>();
                if (locationType == ProcessedLocationType.Polygon && !string.IsNullOrEmpty(locationId))
                {
                    polygonLocationIds.Add(locationId);
                }
            }
        }

        return await GetLocationGeometriesAsync(polygonLocationIds);
    }

    public async Task<Dictionary<string, Geometry>?> GetPolygonLocationGeometriesAsync(IEnumerable<(ProcessedLocationType? Type, IEnumerable<string> LocationIds)> locations)
    {
        var polygonLocationIds = locations
            .Where(location => location.Type == ProcessedLocationType.Polygon)
            .SelectMany(location => location.LocationIds)
            .Where(locationId => !string.IsNullOrWhiteSpace(locationId))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return await GetLocationGeometriesAsync(polygonLocationIds);
    }

    private async Task<Dictionary<string, Geometry>?> GetLocationGeometriesAsync(HashSet<string> polygonLocationIds)
    {
        if (polygonLocationIds.Count == 0)
        {
            return null;
        }

        return await _locationManager.GetLocationGeometryAsync(polygonLocationIds);
    }
}
