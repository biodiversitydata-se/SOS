using AgileObjects.AgileMapper.Extensions;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Analysis.Api.Repositories.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Search;
using System.Globalization;

namespace SOS.Analysis.Api.Managers
{
    public class AnalysisManager : Interfaces.IAnalysisManager
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;        
        private readonly IFilterManager _filterManager;
        private readonly IAreaCache _areaCache;
        private readonly ILogger<AnalysisManager> _logger;

        /// <summary>
        /// Calculate some metadata for grid cells
        /// </summary>
        /// <param name="gridCells"></param>
        /// <returns></returns>
        private (long ObservationsCount, double MinX, double MaxX, double MinY, double MaxY) CalculateMetadata(IEnumerable<GridCell> gridCells)
        {
            double? minX = null, minY = null, maxX = null, maxY = null;
            var observationsCount = 0L;
            foreach (var gridCell in gridCells)
            {
                observationsCount += gridCell.ObservationsCount ?? 0;
                if (minY == null || gridCell.MetricBoundingBox.BottomRight.Y < minY)
                {
                    minY = gridCell.MetricBoundingBox.BottomRight.Y;
                }
                if (maxX == null || gridCell.MetricBoundingBox.BottomRight.X > maxX)
                {
                    maxX = gridCell.MetricBoundingBox.BottomRight.X;
                }
                if (maxY == null || gridCell.MetricBoundingBox.TopLeft.Y > maxY)
                {
                    maxY = gridCell.MetricBoundingBox.TopLeft.Y;
                }
                if (minX == null || gridCell.MetricBoundingBox.TopLeft.X < minX)
                {
                    minX = gridCell.MetricBoundingBox.TopLeft.X;
                }
            }

            return (observationsCount, minX ?? 0.0, maxX ?? 0.0, minY ?? 0.0, maxY ?? 0.0);
        }

        /// <summary>
        /// Get grid cells
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <returns></returns>
        private async Task<IEnumerable<GridCell>> GetGridCellsAync(int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            MetricCoordinateSys metricCoordinateSys)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var gridCells = new List<GridCell>();
            const int pageSize = 10000;
            var result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters, metricCoordinateSys, false, pageSize);
            while ((result?.GridCellCount ?? 0) > 0)
            {
                gridCells.AddRange(result!.GridCells);
                result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters, metricCoordinateSys, false, pageSize, result.AfterKey);
            }

            return gridCells;
        }

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="filterManager"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="areaCache"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnalysisManager(
            IFilterManager filterManager,
            IProcessedObservationRepository processedObservationRepository,
            IAreaCache areaCache,
            ILogger<AnalysisManager> logger)
        {
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<PagedAggregationResultDto<UserAggregationResponseDto>?> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            string? afterKey,
            int? take)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var result = await _processedObservationRepository.AggregateByUserFieldAsync(filter, aggregationField, precisionThreshold, afterKey, take);


            return new PagedAggregationResultDto<UserAggregationResponseDto>
            {
                AfterKey = result.SearchAfter,
                Records = result?.Records?.Select(r => new UserAggregationResponseDto
                {
                    AggregationField = r.AggregationField,
                    Count = (int)r.DocCount,
                    UniqueTaxon = (int)r.UniqueTaxon
                })!
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AggregationItemDto>?> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            int take,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var result = await _processedObservationRepository.GetAggregationItemsAsync(filter, aggregationField, precisionThreshold ?? 40000, take, sortOrder);

            return result?.Select(i => new AggregationItemDto { AggregationKey = i.AggregationKey, DocCount = i.DocCount })!;
        }

        /// <inheritdoc/>
        public async Task<FeatureCollection> AtlasAggregateAsync(
        int? roleId,
        string? authorizationApplicationIdentifier,
        SearchFilter filter,
        AtlasAreaSizeDto atlasSize)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var aggregationField = $"location.{(atlasSize == AtlasAreaSizeDto.Km5x5 ? "atlas5x5" : "atlas10x10")}.featureId";

            var result = await _processedObservationRepository.AggregateByUserFieldAsync(filter, aggregationField, precisionThreshold: 40000, afterKey: null, 10000);
            var futureCollection = new FeatureCollection();
            while (result?.Records?.Any() ?? false)
            {
                foreach (var record in result.Records)
                {
                    var area = (IGeoShape)await _areaCache.GetGeometryAsync(atlasSize switch { AtlasAreaSizeDto.Km5x5 => AreaType.Atlas5x5, _ => AreaType.Atlas10x10 }, record.AggregationField);
                    futureCollection.Add(
                        area.ToFeature(new Dictionary<string, object> {
                            { "observationCount", (int)record.DocCount },
                            { "taxonCount", (int)record.UniqueTaxon }
                        })
                    );
                }

                result = await _processedObservationRepository.AggregateByUserFieldAsync(filter, aggregationField, precisionThreshold: 40000, afterKey: (string)result.SearchAfter?.FirstOrDefault()!, 10000);
            }

            return futureCollection;
        }

        /// <inheritdoc/>
        public async Task<FeatureCollection> CalculateAooAndEooAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            bool useCenterPoint,
            IEnumerable<double> alphaValues,
            bool useEdgeLengthRatio,
            bool allowHoles,
            bool returnGridCells,
            bool includeEmptyCells,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem)
        {
            try
            {
                var gridCells = await GetGridCellsAync(roleId, authorizationApplicationIdentifier, filter, gridCellsInMeters, metricCoordinateSys);
                if (!gridCells.Any())
                {
                    return null!;
                }

                var metaData = CalculateMetadata(gridCells);
                var futureCollection = new FeatureCollection
                {
                    BoundingBox = new Envelope(new Coordinate(metaData.MinX, metaData.MaxY), new Coordinate(metaData.MaxX, metaData.MinY)).Transform((CoordinateSys)metricCoordinateSys, coordinateSystem)
                };

                // We need features to return later so we create them now 
                var gridCellFeaturesMetric = gridCells.Select(gc => gc.MetricBoundingBox
                    .ToPolygon()
                    .ToFeature(new Dictionary<string, object>()
                    {
                        {  "id", GeoJsonHelper.GetGridCellId(gridCellsInMeters, (int)gc.MetricBoundingBox.TopLeft.X, (int)gc.MetricBoundingBox.BottomRight.Y) },
                        {  "observationsCount", gc.ObservationsCount! },
                        {  "taxaCount", gc.TaxaCount! }
                    })
                ).ToDictionary(f => (string)f.Attributes["id"], f => f);

                var metricEooGeometries = new Dictionary<double, Geometry>();
                foreach (var alphaValue in alphaValues)
                {
                    var eooGeometry = gridCellFeaturesMetric
                        .Select(f => f.Value.Geometry as Polygon)
                            .ToArray()
                                .ConcaveHull(useCenterPoint, alphaValue, useEdgeLengthRatio, allowHoles);

                    if (eooGeometry == null)
                    {
                        return null!;
                    }
                    metricEooGeometries.Add(alphaValue, eooGeometry);

                    var gridCellArea = gridCellsInMeters * gridCellsInMeters / 1000000; //Calculate area in km2
                    var aoo = Math.Round((double)gridCells.Count() * gridCellArea, 0);
                    var eoo = Math.Round(eooGeometry.Area / 1000000, 0);

                    futureCollection.Add(new Feature(
                        eooGeometry.Transform((CoordinateSys)metricCoordinateSys, coordinateSystem),
                        new AttributesTable(new KeyValuePair<string, object>[] {
                                new KeyValuePair<string, object>("id", $"eoo-{alphaValue.ToString(CultureInfo.InvariantCulture).Replace(',', '.')}"),
                                new KeyValuePair<string, object>("aoo", (int)aoo),
                                new KeyValuePair<string, object>("eoo", (int)eoo),
                                new KeyValuePair<string, object>("gridCellArea", gridCellArea),
                                new KeyValuePair<string, object>("gridCellAreaUnit", "km2"),
                                new KeyValuePair<string, object>("observationsCount", metaData.ObservationsCount)
                            }
                        )
                    ));
                }

                if (!returnGridCells)
                {
                    return futureCollection;
                }

                // Add empty grid cell where no observation was found too complete grid
                if (includeEmptyCells && !useEdgeLengthRatio)
                {
                    GeoJsonHelper.FillInBlanks(
                        gridCellFeaturesMetric,
                        metricEooGeometries,
                        gridCellsInMeters, new[] {
                            new KeyValuePair<string, object>("observationsCount", 0),
                            new KeyValuePair<string, object>("taxaCount", 0)
                        },
                        useCenterPoint
                    );
                }

                // Add all grid cells features
                foreach (var gridCellFeatureMetric in gridCellFeaturesMetric.OrderBy(gc => gc.Key).Select(f => f.Value))
                {
                    gridCellFeatureMetric.Geometry = gridCellFeatureMetric.Geometry.Transform((CoordinateSys)metricCoordinateSys, coordinateSystem);
                    futureCollection.Add(gridCellFeatureMetric);
                }

                return futureCollection;
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e, "Failed to calculate AOO/EOO. To many buckets");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to calculate AOO/EOO.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<FeatureCollection> CalculateAooAndEooArticle17Async(
           int? roleId,
           string? authorizationApplicationIdentifier,
           SearchFilter filter,
           int gridCellsInMeters,
           int maxDistance,
           MetricCoordinateSys metricCoordinateSys,
           CoordinateSys coordinateSystem)
        {
            try
            {
                var gridCellsMetric = await GetGridCellsAync(roleId, authorizationApplicationIdentifier, filter, gridCellsInMeters, metricCoordinateSys);
                if (!gridCellsMetric.Any())
                {
                    return null!;
                }

                var metaData = CalculateMetadata(gridCellsMetric);

                // We need features to return later so we create them now and don't need to create the polygon more than once
                var gridCellFeaturesMetric = gridCellsMetric.Select(gc => gc.MetricBoundingBox
                    .ToPolygon()
                    .ToFeature(new Dictionary<string, object>()
                    {
                        {  "id", GeoJsonHelper.GetGridCellId(gridCellsInMeters, (int)gc.MetricBoundingBox.TopLeft.X, (int)gc.MetricBoundingBox.BottomRight.Y) },
                        {  "observationsCount", gc.ObservationsCount! },
                        {  "taxaCount", gc.TaxaCount! }
                    })
                ).ToDictionary(f => (string)f.Attributes["id"], f => f);

                var futureCollection = new FeatureCollection
                {
                    BoundingBox = new Envelope(new Coordinate(metaData.MinX, metaData.MaxY), new Coordinate(metaData.MaxX, metaData.MinY)).Transform((CoordinateSys)metricCoordinateSys, coordinateSystem)
                };

                var triangels = gridCellsMetric
                    .Select(gc => new XYBoundingBox() {
                        BottomRight = new XYCoordinate(gc.MetricBoundingBox.BottomRight.X, gc.MetricBoundingBox.BottomRight.Y),
                        TopLeft = new XYCoordinate(gc.MetricBoundingBox.TopLeft.X, gc.MetricBoundingBox.TopLeft.Y)
                    }
                    .ToPolygon())
                        .ToArray()
                            .CalculateTraiangels(true);

                var polygonsInRange = new HashSet<Polygon>();
                if (triangels != null)
                {
                    foreach (var triangle in triangels)
                    {
                        foreach (var corrdinate in triangle.Coordinates)
                        {
                            // Save triangels and sides shorter than alpha value
                            var sidesAdded = 0;
                            if (triangle.Coordinates[0].Distance(triangle.Coordinates[1]) <= maxDistance)
                            {
                                polygonsInRange.Add(new Polygon(new LinearRing(new Coordinate[] { triangle.Coordinates[0], triangle.Coordinates[1], triangle.Coordinates[0] })));
                                sidesAdded++;
                            }
                            if (triangle.Coordinates[1].Distance(triangle.Coordinates[2]) <= maxDistance)
                            {
                                polygonsInRange.Add(new Polygon(new LinearRing(new Coordinate[] { triangle.Coordinates[1], triangle.Coordinates[2], triangle.Coordinates[1] })));
                                sidesAdded++;
                            }
                            if (triangle.Coordinates[2].Distance(triangle.Coordinates[3]) <= maxDistance)
                            {
                                polygonsInRange.Add(new Polygon(new LinearRing(new Coordinate[] { triangle.Coordinates[2], triangle.Coordinates[3], triangle.Coordinates[2] })));
                                sidesAdded++;
                            }
                            if (sidesAdded == 3)
                            {
                                polygonsInRange.Add((Polygon)triangle);
                            }
                        }
                    }
                }

                // Create a multipolygon showing matching polygons
                var inRangeGeometry = new MultiPolygon(polygonsInRange.ToArray());

                // Add empty cells
                GeoJsonHelper.FillInBlanks(
                   gridCellFeaturesMetric,
                   gridCellsInMeters, new[] {
                            new KeyValuePair<string, object>("observationsCount", 0),
                            new KeyValuePair<string, object>("taxaCount", 0)
                   }
               );

                // Add all intersections gridcells to feature collection. Add nagative buffer when intersect to prevent gridcell touching corner match
                var eooGridCellFeaturesMetric = gridCellFeaturesMetric.Where(gc =>
                    long.Parse(gc.Value?.Attributes["observationsCount"]?.ToString() ?? "0") > 0 ||
                    gc.Value!.Geometry.Intersects(inRangeGeometry)).Select(f => f.Value);
                var eooGeometry = new MultiPolygon(eooGridCellFeaturesMetric.Select(f => f.Geometry as Polygon).ToArray());

                var gridCellArea = (long)(gridCellsInMeters * (long)gridCellsInMeters) / 1000000; //Calculate area in km2
                var aoo = Math.Round((double)gridCellsMetric.Count() * gridCellArea, 0);
                var eoo = Math.Round(eooGeometry.Area / 1000000, 0);

                futureCollection.Add(new Feature(
                    eooGeometry,
                    new AttributesTable(new KeyValuePair<string, object>[] {
                        new KeyValuePair<string, object>("id", "eoo"),
                        new KeyValuePair<string, object>("aoo", (int)aoo),
                        new KeyValuePair<string, object>("eoo", (int)eoo),
                        new KeyValuePair<string, object>("gridCellArea", gridCellArea),
                        new KeyValuePair<string, object>("gridCellAreaUnit", "km2"),
                        new KeyValuePair<string, object>("observationsCount", metaData.ObservationsCount)
                        }
                    )
                ));

                // Add aoo features to collection
                eooGridCellFeaturesMetric
                    .Where(f => long.Parse(f.Attributes["observationsCount"]?.ToString() ?? "0") > 0)
                        .ForEach(f => futureCollection.Add(f!));

                // Make sure all geometries is in requested coordinate system
                foreach (var feature in futureCollection)
                {
                    feature.Geometry = feature.Geometry.Transform((CoordinateSys)metricCoordinateSys, coordinateSystem);
                }

                return futureCollection;
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e, "Failed to calculate AOO/EOO. To many buckets");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to calculate AOO/EOO.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<long> GetMatchCountAsync(int? roleId, string? authorizationApplicationIdentifier, SearchFilterBase filter)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            return await _processedObservationRepository.GetMatchCountAsync(filter);
        }
    }
}
