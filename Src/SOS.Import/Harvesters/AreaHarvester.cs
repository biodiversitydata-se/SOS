using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Factories.Harvest;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Repositories.Destination.Area.Interfaces;
using SOS.Import.Repositories.Source.Artportalen.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Helpers.Interfaces;

namespace SOS.Import.Harvesters
{
    /// <summary>
    ///     Area factory class
    /// </summary>
    public class AreaHarvester : IAreaHarvester
    {
        private readonly Repositories.Source.Artportalen.Interfaces.IAreaRepository _areaRepository;
        private readonly Repositories.Destination.Area.Interfaces.IAreaRepository _areaProcessedRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly ILogger<AreaHarvester> _logger;
        private readonly AreaHarvestFactory _harvestFactory;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="logger"></param>
        public AreaHarvester(
            Repositories.Source.Artportalen.Interfaces.IAreaRepository areaRepository,
            Repositories.Destination.Area.Interfaces.IAreaRepository areaProcessedRepository,
            IAreaHelper areaHelper,
            ILogger<AreaHarvester> logger)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _areaProcessedRepository =
                areaProcessedRepository ?? throw new ArgumentNullException(nameof(areaProcessedRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _harvestFactory = new AreaHarvestFactory();
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestAreasAsync()
        {
            var harvestInfo = new HarvestInfo(nameof(Area), DataProviderType.ArtportalenObservations, DateTime.Now);
            try
            {
                _logger.LogDebug("Start getting areas");

                var areas = await _areaRepository.GetAsync();
                _logger.LogDebug("Finish getting areas");

                // Make sure we have an empty collection
                if ((areas?.Any() ?? false) && await _areaProcessedRepository.DeleteCollectionAsync())
                {
                    _logger.LogDebug("Start preparing area collection");
                    await _areaProcessedRepository.DropGeometriesAsync();

                    if (await _areaProcessedRepository.AddCollectionAsync())
                    {
                        _logger.LogDebug("Finish preparing area collection");
                        _logger.LogDebug("Start adding areas");

                        if (await _areaProcessedRepository.AddManyAsync(await _harvestFactory.CastEntitiesToVerbatimsAsync(areas)))
                        {
                            _logger.LogDebug("Finish adding areas");

                            _logger.LogDebug("Start casting geometries");
                            var geometries = areas.ToDictionary(a => a.Id, a => a
                                .PolygonWKT?
                                .ToGeometry()
                                .Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84));
                            _logger.LogDebug("Finsih casting geometries");

                            _logger.LogDebug("Start storing geometries");
                            if (await _areaProcessedRepository.StoreGeometriesAsync(geometries))
                            {
                                _logger.LogDebug("Finish storing geometries");

                                _logger.LogDebug("Start clearing cache");
                                _areaHelper.ClearCache();
                                _logger.LogDebug("Finish clearing cache");

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
            return await _harvestFactory.CastEntitiesToVerbatimsAsync(await _areaRepository.GetAsync());
        }
    }
}