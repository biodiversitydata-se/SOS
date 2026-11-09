using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Lib.Enums;
using SOS.Observations.Api.Managers.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SOS.Observations.Api.Services;

/// <summary>Serializes public area geometries obtained through the existing area cache.</summary>
public interface IAreaGeometryService
{
    /// <summary>Gets a GeoJSON Feature and its content validator, or null if the area or geometry is missing.</summary>
    Task<SerializedAreaGeometry?> GetAsync(AreaTypeDto areaType, string featureId, CancellationToken cancellationToken);
}

/// <summary>A GeoJSON representation and its weak ETag, valid across HTTP content encodings.</summary>
public sealed record SerializedAreaGeometry(byte[] Content, string ETag);

internal sealed class AreaGeometryService : IAreaGeometryService
{
    private readonly IAreaManager _areas;

    public AreaGeometryService(IAreaManager areas)
    {
        _areas = areas;
    }

    public async Task<SerializedAreaGeometry?> GetAsync(AreaTypeDto areaType, string featureId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var area = await _areas.GetAreaAsync(areaType, featureId).WaitAsync(cancellationToken);
        if (area == null) return null;
        var geometry = await _areas.GetGeometryAsync((AreaType)areaType, featureId).WaitAsync(cancellationToken);
        if (geometry == null || geometry.IsEmpty) return null;

        var feature = new Feature(geometry, new AttributesTable
        {
            { "FeatureType", "FilterArea.Area" },
            { "AreaType", areaType.ToString() },
            { "AreaTypeId", (int)areaType },
            { "FeatureId", featureId },
            { "Name", area.Name }
        });
        var content = Encoding.UTF8.GetBytes(new GeoJsonWriter().Write(feature));
        var result = new SerializedAreaGeometry(content, $"W/\"{Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant()}\"");
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }
}
