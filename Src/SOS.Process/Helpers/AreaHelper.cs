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
using SOS.Lib.Constants;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.Shared;
using SOS.Process.Models.Cache;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Helpers
{
    public class AreaHelper : Interfaces.IAreaHelper
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly IProcessedFieldMappingRepository _processedFieldMappingRepository;
        private readonly STRtree<IFeature> _strTree;
        private readonly IDictionary<string, PositionLocation> _featureCache;
        private IDictionary<FieldMappingFieldId, IDictionary<object, int>> _fieldMappingsByFeatureId;
        private IDictionary<FieldMappingFieldId, Dictionary<int, FieldMappingValue>> _fieldMappingValueById;
        private const string _cacheFileName = "positionAreas.json";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="processedFieldMappingRepository"></param>
        public AreaHelper(
            IAreaVerbatimRepository areaVerbatimRepository,
            IProcessedFieldMappingRepository processedFieldMappingRepository)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _processedFieldMappingRepository = processedFieldMappingRepository ?? throw new ArgumentNullException(nameof(processedFieldMappingRepository));
            _strTree = new STRtree<IFeature>();

            // Try to get saved cache
            _featureCache = InitializeCache();

            Task.Run(InitializeAsync).Wait();
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
            // Get field mappings
            var fieldMappings = (await _processedFieldMappingRepository.GetFieldMappingsAsync()).ToArray();
            _fieldMappingsByFeatureId = GetGeoRegionFieldMappingDictionaries(fieldMappings);
            _fieldMappingValueById = fieldMappings.ToDictionary(m => m.Id,
                m => m.Values.ToDictionary(v => v.Id, v => v));

            // If tree already initialized, return
            if (_strTree.Count != 0)
            {
                return;
            }

            
            var areas = (await _areaVerbatimRepository.GetBatchBySkipAsync(0)).ToArray();
            var count = areas.Length;
            var totalCount = count;

            while (count != 0)
            {
                foreach (var area in areas)
                {
                    var feature = area.ToFeature();
                    _strTree.Insert(feature.Geometry.EnvelopeInternal, feature);
                }
                
                areas = (await _areaVerbatimRepository.GetBatchBySkipAsync(totalCount + 1)).ToArray();
                count = areas.Length;
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
        public void AddAreaDataToProcessedSightings(IEnumerable<ProcessedSighting> processedSightings)
        {
            foreach (var processedSighting in processedSightings)
            {
                AddAreaDataToProcessedSighting(processedSighting);
            }
        }

        /// <inheritdoc />
        public void AddAreaDataToProcessedSighting(ProcessedSighting processedSighting)
        {
            if (processedSighting.Location == null || processedSighting.Location.DecimalLatitude.Equals(0) && processedSighting.Location.DecimalLongitude.Equals(0))
            {
                return;
            }

            var positionLocation = GetPositionLocation(processedSighting.Location.DecimalLongitude, processedSighting.Location.DecimalLatitude);
            processedSighting.Location.CountyId = ProcessedFieldMapValue.Create(positionLocation.County?.Id);
            processedSighting.Location.MunicipalityId = ProcessedFieldMapValue.Create(positionLocation.Municipality?.Id);
            processedSighting.Location.ParishId = ProcessedFieldMapValue.Create(positionLocation.Parish?.Id);
            processedSighting.Location.ProvinceId = ProcessedFieldMapValue.Create(positionLocation.Province?.Id);
            processedSighting.IsInEconomicZoneOfSweden = positionLocation.EconomicZoneOfSweden;
            SetCountyPartIdByCoordinate(processedSighting);
            SetProvincePartIdByCoordinate(processedSighting);
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
                        var area = new ProcessedArea
                        {
                            Id = (int) feature.Attributes.GetOptionalValue("id"),
                            FeatureId = (int) feature.Attributes.GetOptionalValue("featureId"),
                            Name = (string) feature.Attributes.GetOptionalValue("name")
                        };
                        switch ((AreaType) feature.Attributes.GetOptionalValue("areaType"))
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

            return positionLocation;
        }

        private static void SetProvincePartIdByCoordinate(ProcessedSighting processedSighting)
        {
            // Set ProvincePartIdByCoordinate. Merge lappmarker into Lappland.
            processedSighting.Location.ProvincePartIdByCoordinate = processedSighting.Location.ProvinceId?.Id;
            if (new[]
            {
                (int) ProvinceId.LuleLappmark,
                (int) ProvinceId.LyckseleLappmark,
                (int) ProvinceId.PiteLappmark,
                (int) ProvinceId.TorneLappmark,
                (int) ProvinceId.AseleLappmark
            }.Contains(processedSighting.Location.ProvinceId?.Id ?? 0))
            {
                processedSighting.Location.ProvincePartIdByCoordinate = (int) SpecialProvincePartId.Lappland;
            }
        }

        private static void SetCountyPartIdByCoordinate(ProcessedSighting processedSighting)
        {
            // Set CountyPartIdByCoordinate. Split Kalmar into Öland and Kalmar fastland.
            processedSighting.Location.CountyPartIdByCoordinate = processedSighting.Location.CountyId?.Id;
            if (processedSighting.Location.CountyId?.Id == (int) CountyId.Kalmar)
            {
                if (processedSighting.Location.ProvinceId?.Id == (int) ProvinceId.Oland)
                {
                    processedSighting.Location.CountyPartIdByCoordinate = (int) SpecialCountyPartId.Oland;
                }
                else
                {
                    processedSighting.Location.CountyPartIdByCoordinate = (int) SpecialCountyPartId.KalmarFastland;
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

        private IDictionary<FieldMappingFieldId, IDictionary<object, int>> GetGeoRegionFieldMappingDictionaries(
            ICollection<FieldMapping> verbatimFieldMappings)
        {
            var dic = new Dictionary<FieldMappingFieldId, IDictionary<object, int>>();
            foreach (var fieldMapping in verbatimFieldMappings.Where(m => m.Id.IsGeographicRegionField()))
            {
                var fieldMappings = fieldMapping.ExternalSystemsMapping.FirstOrDefault(m => m.Id == ExternalSystemId.Artportalen);
                if (fieldMappings != null)
                {
                    ExternalSystemMappingField mapping = fieldMappings.Mappings.Single(m => m.Key == FieldMappingKeyFields.FeatureId);
                    var sosIdByValue = mapping.GetIdByValueDictionary();
                    dic.Add(fieldMapping.Id, sosIdByValue);
                }
            }

            return dic;
        }

        public void AddValueDataToGeographicalFields(ProcessedSighting observation)
        {
            SetValue(observation?.Location?.CountyId, _fieldMappingValueById[FieldMappingFieldId.County]);
            SetValue(observation?.Location?.MunicipalityId, _fieldMappingValueById[FieldMappingFieldId.Municipality]);
            SetValue(observation?.Location?.ProvinceId, _fieldMappingValueById[FieldMappingFieldId.Province]);
            SetValue(observation?.Location?.ParishId, _fieldMappingValueById[FieldMappingFieldId.Parish]);
        }

        private void SetValue(ProcessedFieldMapValue val, IDictionary<int, FieldMappingValue> fieldMappingValueById)
        {
            if (val == null) return;
            if (fieldMappingValueById.TryGetValue(val.Id, out FieldMappingValue fieldMappingValue))
            {
                val.Value = fieldMappingValue.Name;
            }
        }
    }
}