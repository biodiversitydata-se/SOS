using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using Newtonsoft.Json;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Cache;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Helpers
{
    public class AreaHelper : IAreaHelper
    {
        private const string CacheFileName = "positionAreas.json";

        private readonly AreaType[] _areaTypes =
            {AreaType.County, AreaType.Province, AreaType.Municipality, AreaType.Parish, AreaType.EconomicZoneOfSweden};

        private IDictionary<string, PositionLocation> _featureCache;
        private readonly IAreaRepository _processedAreaRepository;
        private readonly STRtree<IFeature> _strTree;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedAreaRepository"></param>
        public AreaHelper(
            IAreaRepository processedAreaRepository)
        {
            _processedAreaRepository = processedAreaRepository ??
                                       throw new ArgumentNullException(nameof(processedAreaRepository));
            _strTree = new STRtree<IFeature>();

            // Try to get saved cache
            InitializeCache();

            Task.Run(InitializeAsync).Wait();
        }


        /// <inheritdoc />
        public void AddAreaDataToProcessedObservations(IEnumerable<Observation> processedObservations)
        {
            foreach (var processedObservation in processedObservations)
            {
                AddAreaDataToProcessedObservation(processedObservation);
            }
        }

        /// <inheritdoc />
        public void AddAreaDataToProcessedObservation(Observation processedObservation)
        {
            if (processedObservation.Location == null || !processedObservation.Location.DecimalLatitude.HasValue ||
                !processedObservation.Location.DecimalLongitude.HasValue)
            {
                return;
            }

            var positionLocation = GetPositionLocation(processedObservation.Location.DecimalLongitude.Value,
                processedObservation.Location.DecimalLatitude.Value);
            processedObservation.IsInEconomicZoneOfSweden = positionLocation.EconomicZoneOfSweden;
            SetCountyPartIdByCoordinate(processedObservation);
            SetProvincePartIdByCoordinate(processedObservation);
        }

        public void ClearCache()
        {
            if (File.Exists(CacheFileName))
            {
                File.Delete(CacheFileName);
            }
            InitializeCache();
        }

        /// <inheritdoc />
        public void PersistCache()
        {
            // Update saved cache
            using var file = new StreamWriter(File.Create(CacheFileName), Encoding.UTF8);
            file.Write(JsonConvert.SerializeObject(_featureCache));
        }

        /// <summary>
        ///     Get save cache if it exists
        /// </summary>
        /// <returns></returns>
        private void InitializeCache()
        {
            // Try to get saved cache
            _featureCache = File.Exists(CacheFileName)
                ? JsonConvert.DeserializeObject<IDictionary<string, PositionLocation>>(
                    File.ReadAllText(CacheFileName, Encoding.UTF8))
                : new ConcurrentDictionary<string, PositionLocation>();
        }

        private async Task InitializeAsync()
        {
            // If tree already initialized, return
            if (_strTree.Count != 0)
            {
                return;
            }

            var areas = await _processedAreaRepository.GetAsync(_areaTypes);
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
                var features = GetPointFeatures(decimalLongitude, decimalLatitude);
                positionLocation = new PositionLocation();

                if (features != null)
                {
                    foreach (var feature in features)
                    {
                        Enum.TryParse(typeof(AreaType), feature.Attributes.GetOptionalValue("areaType").ToString(),
                            out var areaType);

                        var area = new Models.Processed.Observation.Area
                        {
                            FeatureId = feature.Attributes.GetOptionalValue("featureId")?.ToString(),
                            Name = feature.Attributes.GetOptionalValue("name")?.ToString()
                        };
                        switch ((AreaType) areaType)
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

                if (!_featureCache.ContainsKey(key))
                {
                    _featureCache.Add(key, positionLocation);
                }
            }

            return positionLocation;
        }

        private static void SetProvincePartIdByCoordinate(Observation processedObservation)
        {
            // Set ProvincePartIdByCoordinate. Merge lappmarker into Lappland.
            processedObservation.Location.ProvincePartIdByCoordinate = processedObservation.Location.Province?.FeatureId;
            if (new[]
            {
                ProvinceIds.LuleLappmark,
                ProvinceIds.LyckseleLappmark,
                ProvinceIds.PiteLappmark,
                ProvinceIds.TorneLappmark,
                ProvinceIds.ÅseleLappmark
            }.Contains(processedObservation.Location.Province?.FeatureId))
            {
                processedObservation.Location.ProvincePartIdByCoordinate = SpecialProvincePartId.Lappland;
            }
        }

        private static void SetCountyPartIdByCoordinate(Observation processedObservation)
        {
            // Set CountyPartIdByCoordinate. Split Kalmar into Öland and Kalmar fastland.
            processedObservation.Location.CountyPartIdByCoordinate = processedObservation.Location.County?.FeatureId;
            if (processedObservation.Location.County?.FeatureId == CountyId.Kalmar)
            {
                if (processedObservation.Location.Province?.FeatureId == ProvinceIds.Öland)
                {
                    processedObservation.Location.CountyPartIdByCoordinate = SpecialCountyPartId.Öland;
                }
                else
                {
                    processedObservation.Location.CountyPartIdByCoordinate = SpecialCountyPartId.KalmarFastland;
                }
            }
        }
    }
}