using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Cache;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Lib.Repositories.Resource.Interfaces;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Lib.Helpers
{
    public class AreaHelper : IAreaHelper
    {
        private readonly AreaType[] _areaTypesInStrTree = {
            AreaType.County, 
            AreaType.Province, 
            AreaType.Municipality, 
            AreaType.Parish, 
            AreaType.EconomicZoneOfSweden
        };

        private IDictionary<string, PositionLocation> _featureCache;
        private readonly IAreaRepository _processedAreaRepository;
        private STRtree<IFeature> _strTree;
        private SemaphoreSlim _initializeSemaphoreSlim = new SemaphoreSlim(1, 1);
        private static readonly object _lockObject = new object();

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedAreaRepository"></param>
        public AreaHelper(
            IAreaRepository processedAreaRepository)
        {
            _processedAreaRepository = processedAreaRepository ??
                                       throw new ArgumentNullException(nameof(processedAreaRepository));

            ClearCache();
        }

        /// <inheritdoc />
        public void AddAreaDataToProcessedLocation(Location processedLocation)
        {
            if ((processedLocation?.DecimalLongitude ?? 0) == 0
                || (processedLocation?.DecimalLatitude ?? 0) == 0)
            {
                return;
            }

            var positionLocation = GetPositionLocation(processedLocation.DecimalLongitude.Value,
                processedLocation.DecimalLatitude.Value);
            processedLocation.County = positionLocation?.County;
            processedLocation.Municipality = positionLocation?.Municipality;
            processedLocation.Parish = positionLocation?.Parish;
            processedLocation.Province = positionLocation?.Province;
            processedLocation.IsInEconomicZoneOfSweden = positionLocation?.EconomicZoneOfSweden ?? false;

            processedLocation.Attributes.ProvincePartIdByCoordinate =
                GetProvincePartIdByCoordinate(processedLocation.Province?.FeatureId);

            processedLocation.Attributes.CountyPartIdByCoordinate = GetCountyPartIdByCoordinate(
                processedLocation.County?.FeatureId, processedLocation.Province?.FeatureId);
        }

        /// <inheritdoc />
        public void AddAreaDataToProcessedLocations(IEnumerable<Location> processedLocations)
        {
            foreach (var processedLocation in processedLocations)
            {
                AddAreaDataToProcessedLocation(processedLocation);
            }
        }

        /// <inheritdoc />
        public void AddAreaDataToSite(Site site)
        {
            if (site.Point == null)
            {
                return;
            }

            site.ProvincePartIdByCoordinate =
                GetProvincePartIdByCoordinate(site.Province?.FeatureId);

            site.CountyPartIdByCoordinate = GetCountyPartIdByCoordinate(
               site.County?.FeatureId, site.Province?.FeatureId);
        }

        public void ClearCache()
        {
            IsInitialized = false;
            _featureCache = new ConcurrentDictionary<string, PositionLocation>();
            _strTree = new STRtree<IFeature>();
        }

        /// <inheritdoc />
        public async Task InitializeAsync()
        {
            // If tree already initialized, return
            if (IsInitialized) return;
                       
            try
            {
                await _initializeSemaphoreSlim.WaitAsync();
                if (!IsInitialized)
                {
                    ClearCache();

                    var areas = await _processedAreaRepository.GetAsync(_areaTypesInStrTree);
                    foreach (var area in areas)
                    {
                        var geometry = await _processedAreaRepository.GetGeometryAsync(area.AreaType, area.FeatureId);

                        var attributes = new Dictionary<string, object>();
                        attributes.Add("name", area.Name);
                        attributes.Add("areaType", area.AreaType);
                        attributes.Add("featureId", area.FeatureId);

                        var feature = geometry.ToFeature(attributes);
                        _strTree.Insert(feature.Geometry.EnvelopeInternal, feature);
                    }
                    
                    _strTree.Build();
                    IsInitialized = true;
                }
            }
            finally
            {
                _initializeSemaphoreSlim.Release();
            }
        }

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        public async Task<IEnumerable<Models.Shared.Area>> GetAreasAsync(AreaType type)
        {
            return await _processedAreaRepository.GetAsync(new[] {type});
        }

        /// <inheritdoc />
        public async Task<Geometry> GetGeometryAsync(AreaType type, string featureId)
        {
            return await _processedAreaRepository.GetGeometryAsync(type, featureId);
        }

        /// <inheritdoc />
        public IEnumerable<IFeature> GetPointFeatures(Point point)
        {
            var featuresContainingPoint = new List<IFeature>();
            var possibleFeatures = _strTree.Query(point.EnvelopeInternal);
            foreach (var feature in possibleFeatures)
            {
                if (feature.Geometry.Contains(point))
                {
                    featuresContainingPoint.Add(feature);
                }
            }

            return featuresContainingPoint;
        }

        /// <summary>
        ///     Get all features where position is inside area
        /// </summary>
        /// <param name="longitude"></param>
        /// <param name="latitude"></param>
        /// <returns></returns>
        private IEnumerable<IFeature> GetPointFeatures(double longitude, double latitude)
        {
            var factory = new GeometryFactory();
            var point = factory.CreatePoint(new Coordinate(longitude, latitude));

            var featuresContainingPoint = new List<IFeature>();
            var possibleFeatures = _strTree.Query(point.EnvelopeInternal);
            foreach (var feature in possibleFeatures)
            {
                if (feature.Geometry.Contains(point))
                {
                    featuresContainingPoint.Add(feature);
                }
            }

            return featuresContainingPoint;
        }

        private PositionLocation GetPositionLocation(double decimalLongitude, double decimalLatitude)
        {
            // Round coordinates to 5 decimals (roughly 1m)
            var key = $"{Math.Round(decimalLongitude, 5)}-{Math.Round(decimalLatitude, 5)}";

            // Try to get areas from cache. If areas not found for that position, try to get from repository
            if (!_featureCache.TryGetValue(key, out var positionLocation))
            {
                lock (_lockObject)
                {
                    if (_featureCache.TryGetValue(key, out var calculatedPositionLocation)) return calculatedPositionLocation;
                    var features = GetPointFeatures(decimalLongitude, decimalLatitude);
                    positionLocation = new PositionLocation();

                    if (features != null)
                    {
                        foreach (var feature in features)
                        {
                            if (Enum.TryParse(typeof(AreaType), feature.Attributes.GetOptionalValue("areaType").ToString(),
                                out var areaType))
                            {
                                var area = new Area
                                {
                                    FeatureId = feature.Attributes.GetOptionalValue("featureId")?.ToString(),
                                    Name = feature.Attributes.GetOptionalValue("name")?.ToString()
                                };
                                switch ((AreaType)areaType)
                                {
                                    case AreaType.County:
                                        positionLocation.County = area;
                                        break;
                                    case AreaType.Municipality:
                                        positionLocation.Municipality = area;
                                        break;
                                    case AreaType.Parish:
                                        positionLocation.Parish = area;
                                        break;
                                    case AreaType.Province:
                                        positionLocation.Province = area;
                                        break;
                                    case AreaType.EconomicZoneOfSweden:
                                        positionLocation.EconomicZoneOfSweden = true;
                                        break;
                                }
                            }
                        }
                    }

                    if (!_featureCache.ContainsKey(key))
                    {
                        _featureCache.TryAdd(key, positionLocation);
                    }
                }
            }

            return positionLocation;
        }

        private static string GetProvincePartIdByCoordinate(string provinceFeatureId)
        {
            if (new[]
            {
                ProvinceIds.LuleLappmark,
                ProvinceIds.LyckseleLappmark,
                ProvinceIds.PiteLappmark,
                ProvinceIds.TorneLappmark,
                ProvinceIds.ÅseleLappmark
            }.Contains(provinceFeatureId))
            {
               return SpecialProvincePartId.Lappland;
            }

            return provinceFeatureId;
        }

        private static string GetCountyPartIdByCoordinate(string countyFeatureId, string provinceFeatureId)
        {
            if (countyFeatureId == CountyId.Kalmar)
            {
                if (provinceFeatureId == ProvinceIds.Öland)
                {
                    return SpecialCountyPartId.Öland;
                }
                else
                {
                    return SpecialCountyPartId.KalmarFastland;
                }
            }

            return countyFeatureId;
        }
    }
}