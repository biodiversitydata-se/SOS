using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Observations.Api.Repositories.Interfaces;
using SOS.Lib.Models.Shared;
using System.Collections.Generic;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    /// Process info manager
    /// </summary>
    public class AreaManager : Interfaces.IAreaManager
    {
        private readonly IAreaRepository _areaRepository;

        private readonly ILogger<AreaManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processInfoRepository"></param>
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
        public async Task<PagedAreas> GetAreasAsync(int skip, int take)
        {
            try
            {
                var areas = await _areaRepository.GetAllPagedAsync(skip, take);
                var pagedAreas = new PagedAreas();
                pagedAreas.TotalCount = areas.TotalCount;
                var pagedAreaList = new List<PagedArea>();
                foreach(var area in areas.Areas)
                {
                    var pagedArea = new PagedArea()
                    {
                        Id = area.Id,
                        Name = area.Name,
                        AreaType = area.AreaType.ToString(),                        
                    };
                    pagedAreaList.Add(pagedArea);
                }
                pagedAreas.Areas = pagedAreaList;
                return pagedAreas;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get current process info");
                return null;
            }
        }
    }
}
