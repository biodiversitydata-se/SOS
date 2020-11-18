using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Models.Area;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public class AreaManager : IAreaManager
    {
        private readonly IAreaRepository _areaRepository;

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

        private async Task<byte[]> GetZipppedAreaAsync(Area area){
            try
            {
                if (area?.AreaType == AreaType.EconomicZoneOfSweden)
                {
                    return null;
                }

                var geometry = await _areaRepository.GetGeometryAsync(area.Id);
                var externalArea = new ExternalArea
                {
                    AreaType = area.AreaType.ToString(),
                    Feature = area.FeatureId,
                    Geometry = geometry.ToGeoJson(),
                    Id = area.Id,
                    Name = area.Name
                };
                var result = JsonConvert.SerializeObject(externalArea, Formatting.Indented,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return CreateZipFile($"area{area.Id}.json", Encoding.UTF8.GetBytes(result));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get area");
                return null;
            }
        }

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="areaRepository"></param>
    /// <param name="logger"></param>
    public AreaManager(
            IAreaRepository areaRepository,
            ILogger<AreaManager> logger)
        {
            _areaRepository = areaRepository ??
                              throw new ArgumentNullException(nameof(_areaRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<byte[]> GetZipppedAreaAsync(int areaId)
        {
            var area = await _areaRepository.GetAsync(areaId);
            return await GetZipppedAreaAsync(area);
        }

        public async Task<byte[]> GetZipppedAreaAsync(AreaType areaType, string feature)
        {
            var area = await _areaRepository.GetAsync(areaType, feature);
            return await GetZipppedAreaAsync(area);
        }

        /// <inheritdoc />
        public async Task<PagedResult<ExternalSimpleArea>> GetAreasAsync(IEnumerable<AreaType> areaTypes,
            string searchString, int skip, int take)
        {
            try
            {
                var result = await _areaRepository.GetAreasAsync(areaTypes, searchString, skip, take);

                return new PagedResult<ExternalSimpleArea>
                {
                    Records = result.Records.Select(r => new ExternalSimpleArea
                    {
                        AreaType = r.AreaType.ToString(),
                        Feature = r.FeatureId,
                        Id = r.Id,
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