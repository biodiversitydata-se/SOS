using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Import.Factories.Harvest;
using SOS.Import.Harvesters.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;


namespace SOS.Import.Harvesters
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
        private readonly AreaHarvestConfiguration _areaHarvestConfiguration;
        private readonly ILogger<AreaHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="areaRepository"></param>
        /// <param name="areaHarvestConfiguration"></param>
        /// <param name="logger"></param>
        /// <param name="areaProcessedRepository"></param>
        /// <param name="areaHelper"></param>
        /// <param name="geoRegionApiService"></param>
        public AreaHarvester(
            Repositories.Source.Artportalen.Interfaces.IAreaRepository areaRepository,
            IAreaRepository areaProcessedRepository,
            IAreaHelper areaHelper,
            IGeoRegionApiService geoRegionApiService,
            AreaHarvestConfiguration areaHarvestConfiguration,
            ILogger<AreaHarvester> logger)
        {
            _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
            _areaProcessedRepository =
                areaProcessedRepository ?? throw new ArgumentNullException(nameof(areaProcessedRepository));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
            _geoRegionApiService = geoRegionApiService ?? throw new ArgumentNullException(nameof(geoRegionApiService));
            _areaHarvestConfiguration = areaHarvestConfiguration ?? throw new ArgumentNullException(nameof(areaHarvestConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> HarvestAreasAsync()
        {
            if (_areaHarvestConfiguration.UseGeoRegionApiHarvest)
            {
                return await HarvestAreasWithGeoRegionApiAsync();
            }
            else
            {
                return await HarvestAreasFromArtportalenAsync();
            }
        }

        private async Task<HarvestInfo> HarvestAreasWithGeoRegionApiAsync()
        {
            var harvestInfo = new HarvestInfo(DateTime.Now) { Id = nameof(Area) };
            try
            {
                _logger.LogDebug("Start getting areas");
                var featureCollection = await _geoRegionApiService.GetFeatureCollectionFromZipAsync(Enum.GetValues(typeof(AreaType)).Cast<int>(),4326);
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

        private async Task<HarvestInfo> HarvestAreasFromArtportalenAsync()
        {
            var harvestInfo = new HarvestInfo(DateTime.Now){Id = nameof(Area) };
            try
            {
                _logger.LogDebug("Start getting areas");

                var areaEntities = await _areaRepository.GetAsync();
                _logger.LogDebug("Finish getting areas");

                // Make sure we have an empty collection
                if ((areaEntities?.Any() ?? false) && await _areaProcessedRepository.DeleteCollectionAsync())
                {
                    _logger.LogDebug("Start preparing area collection");
                    await _areaProcessedRepository.DropGeometriesAsync();

                    if (await _areaProcessedRepository.AddCollectionAsync())
                    {
                        _logger.LogDebug("Finish preparing area collection");

                        _logger.LogDebug("Start casting geometries");
                        var geometries = areaEntities.ToDictionary(a => ((AreaType)a.AreaDatasetId).ToAreaId(a.FeatureId), a => a
                            .PolygonWKT?
                            .ToGeometry()
                            .Transform(CoordinateSys.WebMercator, CoordinateSys.WGS84));
                        _logger.LogDebug("Finish casting geometries");

                        var harvestFactory = new AreaHarvestFactory(geometries);
                        var areas = await harvestFactory.CastEntitiesToVerbatimsAsync(areaEntities);

                        _logger.LogDebug("Start adding areas");

                        if (await _areaProcessedRepository.AddManyAsync(areas))
                        {
                            _logger.LogDebug("Finish adding areas");

                            _logger.LogDebug("Start storing geometries");
                            if (await _areaProcessedRepository.StoreGeometriesAsync(geometries))
                            {
                                _logger.LogDebug("Finish storing geometries");

                                await _areaProcessedRepository.CreateIndexAsync();

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
    }
}