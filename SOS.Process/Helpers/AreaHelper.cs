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
        public AreaHelper(IAreaVerbatimRepository areaVerbatimRepository)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));

            _strTree = new STRtree<IFeature>();
            _featureCache = new ConcurrentDictionary<string, IEnumerable<IFeature>>();

            Task.Run(() => InitilaizeAsync()).Wait();
        }

        private async Task InitilaizeAsync()
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

                foreach (var feature in features)
                {
                    if (dwcModel.DynamicProperties == null)
                    {
                        dwcModel.DynamicProperties = new DynamicProperties();
                    }

                    switch ((AreaType)feature.Attributes.GetOptionalValue("areaType"))
                    {
                        case AreaType.County:
                            dwcModel.Location.County = (string)feature.Attributes.GetOptionalValue("name");
                            dwcModel.DynamicProperties.CountyIdByCoordinate = (int)feature.Attributes.GetOptionalValue("featureId");
                            //dwcModel.DynamicProperties.CountyPartIdByCoordinate = ; // todo
                            break;
                        case AreaType.Municipality:
                            dwcModel.Location.Municipality = (string)feature.Attributes.GetOptionalValue("name");
                            dwcModel.DynamicProperties.MunicipalityIdByCoordinate = (int)feature.Attributes.GetOptionalValue("featureId");
                            break;
                        case AreaType.Parish:
                            dwcModel.DynamicProperties.Parish = (string)feature.Attributes.GetOptionalValue("name");
                            dwcModel.DynamicProperties.ParishIdByCoordinate = (int)feature.Attributes.GetOptionalValue("featureId");
                            break;
                        case AreaType.Province:
                            dwcModel.Location.StateProvince = (string)feature.Attributes.GetOptionalValue("name");
                            dwcModel.DynamicProperties.ProvinceIdByCoordinate = (int)feature.Attributes.GetOptionalValue("featureId");
                            //dwcModel.DynamicProperties.ProvincePartIdByCoordinate = ; // todo
                            break;
                    }
                }
            }
        }
    }
}
/*
  
--Update CountyIdByCoordinate and CountyPartIdByCoordinate
UPDATE DarwinCoreObservation SET CountyIdByCoordinate = A.FeatureId, CountyPartIdByCoordinate = A.FeatureId
FROM [SwedishSpeciesObservationResources].[dbo].[Area] A INNER JOIN DarwinCoreObservation D ON A.Polygon.STContains(D.point_GoogleMercator) = 1 AND A.AreaDatasetId = 21
INNER JOIN @SpeciesObservationIdTable S ON S.Id = D.Id

--Update ProvinceIdByCoordinate and ProvincePartIdByCoordinate
UPDATE DarwinCoreObservation SET ProvincePartIdByCoordinate = A.FeatureId, ProvinceIdByCoordinate = A.FeatureId
FROM [SwedishSpeciesObservationResources].[dbo].[Area] A INNER JOIN DarwinCoreObservation D ON A.Polygon.STContains(D.point_GoogleMercator)  = 1 AND A.AreaDatasetId = 16
INNER JOIN @SpeciesObservationIdTable S ON S.Id = D.Id

--Update ProvincePartIdByCoordinate for Lappland
UPDATE DarwinCoreObservation SET ProvinceIdByCoordinate = 100
FROM DarwinCoreObservation D INNER JOIN @SpeciesObservationIdTable S ON  D.Id = S.Id
WHERE ProvincePartIdByCoordinate IN (25,26,27,28,29)

--Update ProvinceIdByCoordinate's for Kalmar
UPDATE DarwinCoreObservation SET CountyPartIdByCoordinate = 
CASE WHEN ProvincePartIdByCoordinate = 4 THEN 101 ELSE 100 END
FROM DarwinCoreObservation D INNER JOIN @SpeciesObservationIdTable S ON  D.Id = S.Id
WHERE D.CountyIdByCoordinate = 8
  
 */
