using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Models.Area;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Text;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    /// Area manager
    /// </summary>
    public class AreaManager : Interfaces.IAreaManager
    {
        private readonly IAreaRepository _areaRepository;

        private readonly ILogger<AreaManager> _logger;

        /// <summary>
        /// Constructor
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

        /// <inheritdoc />
        public async Task<Area> GetAreaInternalAsync(int areaId)
        {
            try
            {
                var area = await _areaRepository.GetAreaAsync(areaId);
                return area;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get area");
                return null;
            }
        }
        public async Task<byte[]> GetZipppedAreaAsync(int areaId)
        {
            try
            {
                var area = await GetAreaInternalAsync(areaId);
                var externalArea = new ExternalArea
                {                    
                    AreaType = area.AreaType.ToString(),
                    Geometry = area.Geometry.ToGeoJson(),
                    Id = area.Id,
                    Name = area.Name                    
                };
                var result = JsonConvert.SerializeObject(externalArea, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                return CreateZipFile($"area{areaId}.json", Encoding.UTF8.GetBytes(result));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get area");
                return null;
            }
        }
        /// <inheritdoc />
        public async Task<PagedResult<ExternalSimpleArea>> GetAreasAsync(AreaType areaType, string searchString, int skip, int take)
        {
            try
            {
                var result = await _areaRepository.GetAreasAsync(areaType, searchString, skip, take);

                return new PagedResult<ExternalSimpleArea>
                {
                    Records =  result.Records.Select(r => new ExternalSimpleArea
                    {
                        AreaType = r.AreaType.ToString(),                        
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
    }
}
