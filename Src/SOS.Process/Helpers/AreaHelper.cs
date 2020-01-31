using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Process.Extensions;
using SOS.Process.Repositories.Source.Interfaces;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using Newtonsoft.Json;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Process.Models.Cache;

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

            var areas = await _areaVerbatimRepository.GetBatchBySkipAsync(0);
            var count = areas.Count();
            var totalCount = count;

            while (count != 0)
            {
                foreach (var area in areas)
                {
                    var feature = area.ToFeature();
                    _strTree.Insert(feature.Geometry.EnvelopeInternal, feature);
                }
                
                areas = await _areaVerbatimRepository.GetBatchBySkipAsync(totalCount + 1);
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
        public void AddAreaDataToProcessed(IEnumerable<ProcessedSighting> processedSightings)
        {
            if (!processedSightings?.Any() ?? true)
            {
                return;
            }

            foreach (var processedSighting in processedSightings)
            {
                if (processedSighting.Location == null || (processedSighting.Location.DecimalLatitude.Equals(0) && processedSighting.Location.DecimalLongitude.Equals(0)))
                {
                    continue;
                }
                
               
                // Round coordinates to 5 decimals (roughly 1m)
                var key = $"{Math.Round(processedSighting.Location.DecimalLongitude, 5)}-{Math.Round(processedSighting.Location.DecimalLatitude, 5)}";

                // Try to get areas from cache. If areas not found for that position, try to get from repository
                if (!_featureCache.TryGetValue(key, out var positionLocation))
                {
                    var features = GetPointFeatures(processedSighting.Location.DecimalLongitude, processedSighting.Location.DecimalLatitude);
                    positionLocation = new PositionLocation();

                    if (features != null)
                    {
                        foreach (var feature in features)
                        {
                            var area = new ProcessedArea
                            {
                                Id = (int)feature.Attributes.GetOptionalValue("featureId"),
                                Name = (string)feature.Attributes.GetOptionalValue("name")
                            };
                            switch ((AreaType)feature.Attributes.GetOptionalValue("areaType"))
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

                    _featureCache.Add(key, positionLocation);
                }

                processedSighting.Location.County = positionLocation.County;
                processedSighting.Location.Municipality = positionLocation.Municipality;
                processedSighting.Location.Parish = positionLocation.Parish;
                processedSighting.Location.Province = positionLocation.Province;
                processedSighting.IsInEconomicZoneOfSweden = positionLocation.EconomicZoneOfSweden;


                // Set CountyPartIdByCoordinate. Split Kalmar into Öland and Kalmar fastland.
                processedSighting.Location.CountyPartIdByCoordinate = processedSighting.Location.County?.Id;
                if (processedSighting.Location.County?.Id == (int)CountyFeatureId.Kalmar)
                {
                    if (processedSighting.Location.Province?.Id == (int)ProvinceFeatureId.Oland)
                    {
                        processedSighting.Location.CountyPartIdByCoordinate = (int)CountyFeatureId.Oland;
                    }
                    else
                    {
                        processedSighting.Location.CountyPartIdByCoordinate = (int)CountyFeatureId.KalmarFastland;
                    }
                }

                // Set ProvincePartIdByCoordinate. Merge lappmarker into Lappland.
                processedSighting.Location.ProvincePartIdByCoordinate = processedSighting.Location.Province?.Id;
                if (new []
                {
                    (int)ProvinceFeatureId.LuleLappmark,
                    (int)ProvinceFeatureId.LyckseleLappmark,
                    (int)ProvinceFeatureId.PiteLappmark,
                    (int)ProvinceFeatureId.TorneLappmark,
                    (int)ProvinceFeatureId.AseleLappmark
                }.Contains(processedSighting.Location.Province?.Id ?? 0))
                {
                    processedSighting.Location.ProvincePartIdByCoordinate = (int) ProvinceFeatureId.Lappland;
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