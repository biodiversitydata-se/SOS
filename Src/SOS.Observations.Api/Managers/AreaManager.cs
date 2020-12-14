using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.JsonConverters;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public class AreaManager : IAreaManager
    {
        private readonly IAreaCache _areaCache;
        private readonly ILogger<AreaManager> _logger;

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
        private async Task<byte[]> GetZipppedAreaAsync(Area area){
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
                    Geometry = geometry.ToGeoJson(),
                    Name = area.Name
                };

                var serializeOptions = new JsonSerializerOptions{ IgnoreNullValues = true };
                serializeOptions.Converters.Add(new GeometryConverter());
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

    public async Task<byte[]> GetZipppedAreaAsync(AreaTypeDto areaType, string featureId)
        {
            var area = await _areaCache.GetAsync(((AreaType)areaType).ToAreaId(featureId));
            return await GetZipppedAreaAsync(area);
        }

        /// <inheritdoc />
        public async Task<PagedResult<AreaBaseDto>> GetAreasAsync(IEnumerable<AreaTypeDto> areaTypes,
            string searchString, int skip, int take)
        {
            try
            {
                var result = await _areaCache.GetAreasAsync(areaTypes.Select(at => (AreaType) at), searchString, skip, take);

                return new PagedResult<AreaBaseDto>
                {
                    Records = result.Records.Select(r => new AreaBaseDto
                    {
                        AreaType = (AreaTypeDto) r.AreaType,
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
    }
}