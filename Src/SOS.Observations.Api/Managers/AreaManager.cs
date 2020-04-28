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
        public async Task<Area> GetAreaAsync(int areaId)
        {
            try
            {
                var processInfo = await _areaRepository.GetAreaAsync(areaId);
                return processInfo;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get current process info");
                return null;
            }
        }
        /// <inheritdoc />
        public async Task<PagedResult<ExternalArea>> GetAreasAsync(AreaType areaType, string searchString, int skip, int take)
        {
            try
            {
                var result = await _areaRepository.GetAreasAsync(areaType, searchString, skip, take);

                return new PagedResult<ExternalArea>
                {
                    Records =  result.Records.Select(r => new ExternalArea
                    {
                        AreaType = r.AreaType.ToString(),
                        Geometry = r.Geometry.ToGeoJson(),
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
