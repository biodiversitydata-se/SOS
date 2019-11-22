using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Process.Extensions;
using SOS.Process.Repositories.Source.Interfaces;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;

namespace SOS.Process.Helpers
{
    public class AreaHelper : Interfaces.IAreaHelper
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly STRtree<IFeature> _strTree;
        private readonly IDictionary<string, IEnumerable<IFeature>> _featureCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaVerbatimRepository"></param>
        /// <param name="areaNameMapper"></param>
        public AreaHelper(IAreaVerbatimRepository areaVerbatimRepository)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _strTree = new STRtree<IFeature>();
            _featureCache = new ConcurrentDictionary<string, IEnumerable<IFeature>>();

            Task.Run(() => InitializeAsync()).Wait();
        }

        private async Task InitializeAsync()
        {
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

                // Round coordinates to 5 decimals (roughly 1m)
                var key = $"{Math.Round(dwcModel.Location.DecimalLongitude, 5)}-{Math.Round(dwcModel.Location.DecimalLatitude, 5)}";

                // Try to get areas from cache
                _featureCache.TryGetValue(key, out var features);

                // If areas not found for that position, try to get from repository
                if (features == null)
                {
                    features = GetPointFeatures(dwcModel.Location.DecimalLongitude, dwcModel.Location.DecimalLatitude);

                    _featureCache.Add(key, features);
                }

                if (features == null)
                {
                    continue;
                }

                if (dwcModel.DynamicProperties == null)
                {
                    dwcModel.DynamicProperties = new DynamicProperties();
                }

                foreach (var feature in features)
                {
                    switch ((AreaType)feature.Attributes.GetOptionalValue("areaType"))
                    {
                        case AreaType.County:
                            dwcModel.Location.County = (string) feature.Attributes.GetOptionalValue("name");
                            dwcModel.DynamicProperties.CountyIdByCoordinate = (int)feature.Attributes.GetOptionalValue("featureId");
                            break;
                        case AreaType.Municipality:
                            dwcModel.Location.Municipality = (string) feature.Attributes.GetOptionalValue("name");
                            dwcModel.DynamicProperties.MunicipalityIdByCoordinate = (int)feature.Attributes.GetOptionalValue("featureId");
                            break;
                        case AreaType.Parish:
                            dwcModel.DynamicProperties.Parish = (string) feature.Attributes.GetOptionalValue("name");
                            dwcModel.DynamicProperties.ParishIdByCoordinate = (int)feature.Attributes.GetOptionalValue("featureId");
                            break;
                        case AreaType.Province:
                            dwcModel.Location.StateProvince = (string) feature.Attributes.GetOptionalValue("name");
                            dwcModel.DynamicProperties.ProvinceIdByCoordinate = (int)feature.Attributes.GetOptionalValue("featureId");
                            break;
                        case AreaType.EconomicZoneOfSweden:
                            dwcModel.IsInEconomicZoneOfSweden = true;
                            break;
                    }
                }

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
    }
}