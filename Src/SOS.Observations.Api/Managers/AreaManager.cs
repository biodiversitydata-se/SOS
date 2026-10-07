using NetTopologySuite.Features;
using NetTopologySuite.IO;
using GeoJSON.Text.Geometry;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Common.Responses;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Managers.Interfaces;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArgumentException = System.ArgumentException;
using NetTopologySuite.Geometries;

namespace SOS.Observations.Api.Managers;

/// <summary>
///     Area manager
/// </summary>
public class AreaManager : IAreaManager
{
    private readonly IAreaCache _areaCache;
    private readonly ILogger<AreaManager> _logger;
    private readonly WKTWriter _wktWriter = new WKTWriter();

    private byte[] CreateZipFile(string filename, byte[] bytes)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var zipEntry = archive.CreateEntry(filename, CompressionLevel.Optimal);
            using var zipStream = zipEntry.Open();
            zipStream.Write(bytes, 0, bytes.Length);
        }

        return ms.ToArray();
    }

    /// <inheritdoc />
    private async Task<byte[]> GetZippedAreaAsync(SOS.Lib.Models.Shared.Area area)
    {
        try
        {
            if (area?.AreaType == AreaType.EconomicZoneOfSweden)
            {
                return null;
            }

            var geometry = await _areaCache.GetGeometryAsync(area.AreaType, area.FeatureId);
            var externalArea = new AreaWithGeometryDto
            {
                AreaType = (AreaTypeDto)area.AreaType,
                FeatureId = area.FeatureId,
                BoundingBox = area.BoundingBox.GetCompleteBoundingBox(),
                Geometry = geometry is null ? null : geometry.ToGeoJsonGeometry(),
                Name = area.Name
            };

            var serializeOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            serializeOptions.Converters.Add(new PascalCaseGeometryConverter()); // The zipped JSON format uses PascalCase geometry properties.
            serializeOptions.Converters.Add(new JsonStringEnumConverter());

            var areaString = JsonSerializer.Serialize(externalArea, serializeOptions);
            return CreateZipFile($"area{area.Id}.json", Encoding.UTF8.GetBytes(areaString));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get area");
            return null;
        }
    }

    private sealed class PascalCaseGeometryConverter : JsonConverter<IGeometryObject>
    {
        public override IGeometryObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException("This converter is only used for area JSON exports.");

        public override void Write(Utf8JsonWriter writer, IGeometryObject value, JsonSerializerOptions options)
        {
            var geometry = JsonSerializer.SerializeToElement(value, value.GetType());
            writer.WriteStartObject();
            foreach (var property in geometry.EnumerateObject().Where(property => property.Name != "type"))
            {
                writer.WritePropertyName(char.ToUpperInvariant(property.Name[0]) + property.Name[1..]);
                property.Value.WriteTo(writer);
            }

            writer.WritePropertyName("Type");
            geometry.GetProperty("type").WriteTo(writer);
            writer.WriteEndObject();
        }
    }

    private static AreaBaseDto ToAreaBaseDto(SOS.Lib.Models.Shared.Area area) => new()
    {
        AreaType = (AreaTypeDto)area.AreaType,
        BoundingBox = area.BoundingBox.GetCompleteBoundingBox(),
        FeatureId = area.FeatureId,
        Name = area.Name
    };

    private async Task<byte[]> GetZippedAreaAsJsonAsync(AreaTypeDto areaType, string featureId)
    {
        var area = await _areaCache.GetAsync(((AreaType)areaType).ToAreaId(featureId));
        return await GetZippedAreaAsync(area);
    }

    private async Task<byte[]> GetZippedAreaAsGeoJsonAsync(AreaTypeDto areaType, string featureId)
    {
        var area = await _areaCache.GetAsync(((AreaType)areaType).ToAreaId(featureId));
        return await GetZippedAreaGeoJsonAsync(area);
    }

    private async Task<byte[]> GetZippedAreaAsWktAsync(AreaTypeDto areaType, string featureId)
    {
        var area = await _areaCache.GetAsync(((AreaType)areaType).ToAreaId(featureId));
        return await GetZippedAreaWktAsync(area);
    }

    private async Task<byte[]> GetZippedAreaGeoJsonAsync(SOS.Lib.Models.Shared.Area area)
    {
        try
        {
            if (area?.AreaType == AreaType.EconomicZoneOfSweden)
            {
                return null;
            }

            var geometry = await _areaCache.GetGeometryAsync(area.AreaType, area.FeatureId);
            
            var attributesTable = new AttributesTable
            {
                { "AreaType", area.AreaType.ToString() },
                { "FeatureId", area.FeatureId },
                { "Name", area.Name },
                { "BoundingBox", area.BoundingBox }
            };
            
            var areaString = geometry.ToFeature(attributesTable).ToGeoJsonString();
            return CreateZipFile($"area{area.Id}.geojson", Encoding.UTF8.GetBytes(areaString));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get area");
            return null;
        }
    }

    private async Task<byte[]> GetZippedAreaWktAsync(SOS.Lib.Models.Shared.Area area)
    {
        try
        {
            if (area?.AreaType == AreaType.EconomicZoneOfSweden)
            {
                return null;
            }

            var geometry = await _areaCache.GetGeometryAsync(area.AreaType, area.FeatureId);
            var geom = geometry;
            var areaString = _wktWriter.Write(geom);
            return CreateZipFile($"area{area.Id}.wkt", Encoding.UTF8.GetBytes(areaString));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get area");
            return null;
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="areaCache"></param>
    /// <param name="logger"></param>
    public AreaManager(
    IAreaCache areaCache,
    ILogger<AreaManager> logger)
    {
        _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AreaBaseDto>> GetAreasAsync(IEnumerable<(AreaTypeDto, string)> areaKeys)
    {
        try
        {
            var areas = await _areaCache.GetAreasAsync(areaKeys.Select(k => ((AreaType)k.Item1, k.Item2)));

            return areas?.Select(ToAreaBaseDto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get paged list of areas");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<AreaBaseDto>> GetAreasAsync(IEnumerable<AreaTypeDto> areaTypes,
        string searchString, int skip, int take)
    {
        try
        {
            var result = await _areaCache.GetAreasAsync(areaTypes?.Select(at => (AreaType)at), searchString, skip, take);

            return new PagedResultDto<AreaBaseDto>
            {
                Records = result.Records?.Select(ToAreaBaseDto).ToList() ?? [],
                Skip = result.Skip,
                Take = result.Take,
                TotalCount = result.TotalCount
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get paged list of areas");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<AreaBaseDto> GetAreaAsync(AreaTypeDto areaType, string featureId)
    {
        try
        {
            var result = await _areaCache.GetAsync((AreaType)areaType, featureId);

            return result is null ? null : ToAreaBaseDto(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get area from cache");
            return null;
        }
    }

    public async Task<byte[]> GetZippedAreaAsync(AreaTypeDto areaType, string featureId, AreaExportFormat format)
    {
        switch (format)
        {
            case AreaExportFormat.Json:
                return await GetZippedAreaAsJsonAsync(areaType, featureId);
            case AreaExportFormat.GeoJson:
                return await GetZippedAreaAsGeoJsonAsync(areaType, featureId);
            case AreaExportFormat.Wkt:
                return await GetZippedAreaAsWktAsync(areaType, featureId);
            default:
                throw new ArgumentException(
                    $"{MethodBase.GetCurrentMethod()?.Name}() does not support the value {areaType}", nameof(areaType));
        }
    }

    /// <inheritdoc />
    public async Task<Geometry> GetGeometryAsync(AreaType areaType, string featureId)
    {
        return await _areaCache.GetGeometryAsync(areaType, featureId);
    }
}
