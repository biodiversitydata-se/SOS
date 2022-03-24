using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;


namespace SOS.Harvest.Harvesters
{
    /// <summary>
    ///     Area factory class
    /// </summary>
    public class AreaHarvester : IAreaHarvester
    {
        private readonly Repositories.Source.Artportalen.Interfaces.IAreaRepository _areaRepository;
        private readonly IGeoRegionApiService _geoRegionApiService;
        private readonly IAreaRepository _areaProcessedRepository;
        private readonly IAreaHelper _areaHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger<AreaHarvester> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="areaProcessedRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="geoRegionApiService"></param>
        /// <param name="cacheManager"></param>
        /// <param name="logger"></param>
        public AreaHarvester(
            Repositories.Source.Artportalen.Interfaces.IAreaRepository areaRepository,
            IAreaRepository areaProcessedRepository,
            IAreaHelper areaHelper,
            IGeoRegionApiService geoRegionApiService,
            ICacheManager cacheManager,
            ILogger<AreaHarvester> logger)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _areaProcessedRepository =
                areaProcessedRepository ?? throw new ArgumentNullException(nameof(areaProcessedRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _geoRegionApiService = geoRegionApiService ?? throw new ArgumentNullException(nameof(geoRegionApiService));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestAreasAsync()
        {
            var harvestInfo = new HarvestInfo(DateTime.Now) { Id = nameof(Area) };
            try
            {
                _logger.LogDebug("Start getting areas");
                var featureCollection = await _geoRegionApiService.GetFeatureCollectionFromZipAsync(Enum.GetValues(typeof(AreaType)).Cast<int>(), 4326);
                _logger.LogDebug("Finish getting areas");

                var areas = new List<Area>();
                var areaGeometries = new Dictionary<string, Geometry>();
                foreach (IFeature feature in featureCollection)
                {
                    var area = new Area((AreaType)Convert.ToInt32(feature.Attributes["AreaDatasetId"]), (string)feature.Attributes["FeatureId"]);
                    area.Name = (string)feature.Attributes["Name"];
                    area.Id = $"{area.AreaType}:{area.FeatureId}";
                    List<double> bbox = ((List<object>)feature.Attributes["Bbox"]).Cast<double>().ToList();
                    area.BoundingBox = new LatLonBoundingBox
                    {
                        TopLeft = new LatLonCoordinate(bbox[3], bbox[0]),
                        BottomRight = new LatLonCoordinate(bbox[1], bbox[2])
                    };
                    areas.Add(area);
                    areaGeometries.Add(area.Id, feature.Geometry);
                }

                // Make sure we have an empty collection
                if (areas.Count > 0 && await _areaProcessedRepository.DeleteCollectionAsync())
                {
                    _logger.LogDebug("Start adding areas");
                    await _areaProcessedRepository.DropGeometriesAsync();
                    if (await _areaProcessedRepository.AddCollectionAsync())
                    {
                        if (await _areaProcessedRepository.AddManyAsync(areas))
                        {
                            if (await _areaProcessedRepository.StoreGeometriesAsync(areaGeometries))
                            {
                                await _areaProcessedRepository.CreateIndexAsync();
                                _areaHelper.ClearCache();
                                _logger.LogDebug("Adding areas succeeded");

                                // Clear observation api cache
                                await _cacheManager.ClearAsync(Cache.Area);

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
    }
}