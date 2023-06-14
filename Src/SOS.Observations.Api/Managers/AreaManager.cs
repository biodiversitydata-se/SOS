using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Managers.Interfaces;
using ArgumentException = System.ArgumentException;

namespace SOS.Observations.Api.Managers
{
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
        private async Task<byte[]> GetZippedAreaAsync(Area area)
        {
            try
            {
                if (area?.AreaType == AreaType.EconomicZoneOfSweden)
                {
                    return null;
                }

                var geometry = await _areaCache.GetGeometryAsync(area.AreaType, area.FeatureId);
                var externalArea = new AreaDto
                {
                    AreaType = (AreaTypeDto)area.AreaType,
                    FeatureId = area.FeatureId,
                    BoundingBox = area.BoundingBox,
                    Geometry = geometry.ToGeoJson(),
                    Name = area.Name
                };

                var serializeOptions = new JsonSerializerOptions {  DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                serializeOptions.Converters.Add(new GeoJsonConverter(true)); // Länsstyrelsen fix. Expects capital letter.
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

        private async Task<byte[]> GetZippedAreaGeoJsonAsync(Area area)
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
                var areaString = GeoJsonHelper.GetFeatureAsGeoJsonString(geometry, attributesTable);
                return CreateZipFile($"area{area.Id}.geojson", Encoding.UTF8.GetBytes(areaString));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get area");
                return null;
            }
        }

        private async Task<byte[]> GetZippedAreaWktAsync(Area area)
        {
            try
            {
                if (area?.AreaType == AreaType.EconomicZoneOfSweden)
                {
                    return null;
                }

                var geometry = await _areaCache.GetGeometryAsync(area.AreaType, area.FeatureId);
                var geom = geometry.ToGeometry();
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

                return areas?.Select(a => new AreaBaseDto{ AreaType = (AreaTypeDto)a.AreaType, FeatureId = a.FeatureId, Name = a.Name, BoundingBox = a.BoundingBox });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get paged list of areas");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<PagedResult<AreaBaseDto>> GetAreasAsync(IEnumerable<AreaTypeDto> areaTypes,
            string searchString, int skip, int take)
        {
            try
            {
                var result = await _areaCache.GetAreasAsync(areaTypes.Select(at => (AreaType)at), searchString, skip, take);

                return new PagedResult<AreaBaseDto>
                {
                    Records = result.Records.Select(r => new AreaBaseDto
                    {
                        AreaType = (AreaTypeDto)r.AreaType,
                        BoundingBox = r.BoundingBox,
                        FeatureId = r.FeatureId,
                        Name = r.Name
                    }),
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

                return result == null ? null : new AreaBaseDto
                {
                    AreaType = (AreaTypeDto)result.AreaType,
                    BoundingBox = result.BoundingBox,
                    FeatureId = result.FeatureId,
                    Name = result.Name
                };
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
        public async Task<IGeoShape> GetGeometryAsync(AreaType areaType, string featureId)
        {
            return await _areaCache.GetGeometryAsync(areaType, featureId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IGeoShape>> GetGeometriesAsync(IEnumerable<(AreaType areaType, string featureId)> areaKeys)
        {
            return await _areaCache.GetGeometriesAsync(areaKeys);
        }
    }
}