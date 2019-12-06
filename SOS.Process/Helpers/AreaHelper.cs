using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using SOS.Process.Extensions;
using SOS.Process.Repositories.Source.Interfaces;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using Newtonsoft.Json;
using SOS.Process.Models.Cache;

using Location = SOS.Process.Models.Cache.Location;

namespace SOS.Process.Helpers
{
    public class AreaHelper : Interfaces.IAreaHelper
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly STRtree<IFeature> _strTree;
        private readonly IDictionary<string, PositionLocation> _featureCache;
        private const string _cacheFileName = "positionAreas.json";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="cacheRepository"></param>
        public AreaHelper(IAreaVerbatimRepository areaVerbatimRepository)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _strTree = new STRtree<IFeature>();

            // Try to get saved cache
            _featureCache = InitializeCache();

            Task.Run(() => InitializeAsync()).Wait();
        }

        /// <summary>
        /// Get save cache if it exists
        /// </summary>
        /// <returns></returns>
        private static IDictionary<string, PositionLocation> InitializeCache()
        {
            // Try to get saved cache
           return File.Exists(_cacheFileName) ?
                JsonConvert.DeserializeObject<IDictionary<string, PositionLocation>>(
                    File.ReadAllText(_cacheFileName, Encoding.UTF8)) : new ConcurrentDictionary<string, PositionLocation>();
        }

        private async Task InitializeAsync()
        {
            // If tree already initialized, return
            if (_strTree.Count != 0)
            {
                return;
            }

            var areas = await _areaVerbatimRepository.GetBatchAsync(0);
            var count = areas.Count();
            var totalCount = count;

            while (count != 0)
            {
                foreach (var area in areas)
                {
                    var feature = area.ToFeature();
                    _strTree.Insert(feature.Geometry.EnvelopeInternal, feature);
                }
                
                areas = await _areaVerbatimRepository.GetBatchAsync(totalCount + 1);
                count = areas.Count();
                totalCount += count;
            }

            _strTree.Build();
        }

        /// <summary>
        /// Get all features where position is inside area
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

        /// <inheritdoc />
        public void AddAreaDataToDarwinCore(IEnumerable<DarwinCore<DynamicProperties>> darwinCoreModels)
        {
            if (!darwinCoreModels?.Any() ?? true)
            {
                return;
            }

            foreach (var dwcModel in darwinCoreModels)
            {
                if (dwcModel.Location == null || (dwcModel.Location.DecimalLatitude.Equals(0) && dwcModel.Location.DecimalLongitude.Equals(0)))
                {
                    continue;
                }
                
                if (dwcModel.DynamicProperties == null)
                {
                    dwcModel.DynamicProperties = new DynamicProperties();
                }

                // Round coordinates to 5 decimals (roughly 1m)
                var key = $"{Math.Round(dwcModel.Location.DecimalLongitude, 5)}-{Math.Round(dwcModel.Location.DecimalLatitude, 5)}";

                // Try to get areas from cache. If areas not found for that position, try to get from repository
                if (!_featureCache.TryGetValue(key, out var positionLocation))
                {
                    var features = GetPointFeatures(dwcModel.Location.DecimalLongitude, dwcModel.Location.DecimalLatitude);
                    positionLocation = new PositionLocation();

                    if (features != null)
                    {
                        foreach (var feature in features)
                        {
                            var location = new Location
                            {
                                Id = (int)feature.Attributes.GetOptionalValue("featureId"),
                                Name = (string)feature.Attributes.GetOptionalValue("name")
                            };
                            switch ((AreaType)feature.Attributes.GetOptionalValue("areaType"))
                            {
                                case AreaType.County:
                                    positionLocation.County = location;
                                    break;
                                case AreaType.Municipality:
                                    positionLocation.Municipality = location;
                                    break;
                                case AreaType.Parish:
                                    positionLocation.Parish = location;
                                    break;
                                case AreaType.Province:
                                    positionLocation.Province = location;
                                    break;
                                case AreaType.EconomicZoneOfSweden:
                                    positionLocation.EconomicZoneOfSweden = true;
                                    break;
                            }
                        }
                    }

                    _featureCache.Add(key, positionLocation);
                }

                dwcModel.Location.County = positionLocation.County?.Name;
                dwcModel.DynamicProperties.CountyIdByCoordinate = positionLocation.County?.Id;
                dwcModel.Location.Municipality = positionLocation.Municipality?.Name;
                dwcModel.DynamicProperties.MunicipalityIdByCoordinate = positionLocation.Municipality?.Id;
                dwcModel.DynamicProperties.Parish = positionLocation.Parish?.Name;
                dwcModel.DynamicProperties.ParishIdByCoordinate = positionLocation.Parish?.Id;
                dwcModel.Location.StateProvince = positionLocation.Province?.Name;
                dwcModel.DynamicProperties.ProvinceIdByCoordinate = positionLocation.Province?.Id;
                dwcModel.IsInEconomicZoneOfSweden = positionLocation.EconomicZoneOfSweden;

                // Set CountyPartIdByCoordinate. Split Kalmar into Öland and Kalmar fastland.
                dwcModel.DynamicProperties.CountyPartIdByCoordinate = dwcModel.DynamicProperties.CountyIdByCoordinate;
                if (dwcModel.DynamicProperties.CountyIdByCoordinate == (int)CountyFeatureId.Kalmar)
                {
                    if (dwcModel.DynamicProperties.ProvinceIdByCoordinate == (int)ProvinceFeatureId.Oland)
                    {
                        dwcModel.DynamicProperties.CountyPartIdByCoordinate = (int) CountyFeatureId.Oland;
                    }
                    else
                    {
                        dwcModel.DynamicProperties.CountyPartIdByCoordinate = (int)CountyFeatureId.KalmarFastland;
                    }
                }

                // Set ProvincePartIdByCoordinate. Merge lappmarker into Lappland.
                dwcModel.DynamicProperties.ProvincePartIdByCoordinate = dwcModel.DynamicProperties.ProvinceIdByCoordinate;
                if (dwcModel.DynamicProperties.ProvinceIdByCoordinate == (int)ProvinceFeatureId.LuleLappmark ||
                    dwcModel.DynamicProperties.ProvinceIdByCoordinate == (int)ProvinceFeatureId.LyckseleLappmark ||
                    dwcModel.DynamicProperties.ProvinceIdByCoordinate == (int)ProvinceFeatureId.PiteLappmark ||
                    dwcModel.DynamicProperties.ProvinceIdByCoordinate == (int)ProvinceFeatureId.TorneLappmark ||
                    dwcModel.DynamicProperties.ProvinceIdByCoordinate == (int)ProvinceFeatureId.AseleLappmark)
                {
                    dwcModel.DynamicProperties.ProvincePartIdByCoordinate = (int) ProvinceFeatureId.Lappland;
                }
            }
        }

        /// <inheritdoc />
        public void PersistCache()
        {
            // Update saved cache
            using var file = new StreamWriter(File.Create(_cacheFileName), Encoding.UTF8);
            file.Write(JsonConvert.SerializeObject(_featureCache));
        }
    }
}