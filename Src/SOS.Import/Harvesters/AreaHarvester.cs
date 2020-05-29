using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Extensions;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Repositories.Destination.Artportalen.Interfaces;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters
{
    /// <summary>
    ///     Area factory class
    /// </summary>
    public class AreaHarvester : IAreaHarvester
    {
        private readonly IAreaRepository _areaRepository;
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly ILogger<AreaHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="logger"></param>
        public AreaHarvester(
            IAreaRepository areaRepository,
            IAreaVerbatimRepository areaVerbatimRepository,
            ILogger<AreaHarvester> logger)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _areaVerbatimRepository =
                areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestAreasAsync()
        {
            var harvestInfo = new HarvestInfo(nameof(Area), DataProviderType.ArtportalenObservations, DateTime.Now);
            try
            {
                var start = DateTime.Now;
                _logger.LogDebug("Start getting areas");

                var areas = await _areaRepository.GetAsync();
                _logger.LogDebug("Finish getting areas");

                // Make sure we have an empty collection
                if ((areas?.Any() ?? false) && await _areaVerbatimRepository.DeleteCollectionAsync())
                {
                    _logger.LogDebug("Start preparing area collection");
                    await _areaVerbatimRepository.DropGeometriesAsync();

                    if (await _areaVerbatimRepository.AddCollectionAsync())
                    {
                        _logger.LogDebug("Finish preparing area collection");
                        _logger.LogDebug("Start adding areas");
                        if (await _areaVerbatimRepository.AddManyAsync(areas.ToVerbatims()))
                        {
                            _logger.LogDebug("Finish adding areas");

                            _logger.LogDebug("Start casting geometries");
                            var geometries = areas.ToDictionary(a => a.Id, a => a.Polygon.ToGeometry());
                            _logger.LogDebug("Finsih casting geometries");

                            _logger.LogDebug("Start storing geometries");
                            if (await _areaVerbatimRepository.StoreGeometriesAsync(geometries))
                            {
                                _logger.LogDebug("Finish storing geometries");
                                _logger.LogDebug("Adding areas succeeded");

                                // Update harvest info
                                harvestInfo.End = DateTime.Now;
                                harvestInfo.Status = RunStatus.Success;
                                harvestInfo.Count = areas?.Count() ?? 0;

                                return harvestInfo;
                            }
                        }
                    }
                }

                _logger.LogDebug("Failed harvest of areas");
                harvestInfo.Status = RunStatus.Failed;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of areas");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Area>> GetAreasAsync()
        {
            return (await _areaRepository.GetAsync()).ToVerbatims();
        }
    }
}