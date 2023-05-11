using AgileObjects.AgileMapper.Extensions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Analysis.Api.Dtos.Search;
using SOS.Analysis.Api.Managers.Interfaces;
using SOS.Analysis.Api.Repositories.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.Managers
{
    public class AnalysisManager : IAnalysisManager
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFilterManager _filterManager;
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
        ///  Constructor
        /// </summary>
        /// <param name="filterManager"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnalysisManager(
            IFilterManager filterManager,
            IProcessedObservationRepository processedObservationRepository,
            ILogger<AnalysisManager> logger)
        {
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<PagedAggregationResultDto<UserAggregationResponseDto>> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter, 
            string aggregationField,
            string? afterKey,
            int? take)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var result = await _processedObservationRepository.AggregateByUserFieldAsync(filter, aggregationField, afterKey, take);

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
        public async Task<FeatureCollection> CalculateAooAndEooAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            bool useCenterPoint,
            IEnumerable<double> edgeLengths,
            bool useEdgeLengthRatio,
            bool allowHoles,
            bool returnGridCells,
            bool includeEmptyCells,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                var gridCells = new List<GridCell>();
                const int pageSize = 10000;
                var result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters, metricCoordinateSys, false, pageSize);
                while((result?.GridCellCount ?? 0) > 0)
                {
                    gridCells.AddRange(result!.GridCells);
                    result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters, metricCoordinateSys, false, pageSize, result.AfterKey);
                }
                var metaData = CalculateMetadata(gridCells);
                if (!gridCells.Any())
                {
                    return null!;
                }
                    
                var gridCellCount = gridCells.Count();
                var gridCellArea = gridCellsInMeters * gridCellsInMeters / 1000000; //Calculate area in km2
                var aoo = Math.Round((double)gridCellCount * gridCellArea, 0);

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

                var futureCollection = new FeatureCollection { 
                    BoundingBox = new Envelope(new Coordinate(metaData.MinX, metaData.MaxY), new Coordinate(metaData.MaxX, metaData.MinY)).Transform((CoordinateSys)metricCoordinateSys, coordinateSystem) 
                };

                foreach (var edgeLength in edgeLengths)
                {
                    var eooGeometry = gridCellFeaturesMetric
                        .Select(f => f.Value.Geometry as Polygon)
                            .ToArray()
                                .ConcaveHull(useCenterPoint, edgeLength, useEdgeLengthRatio, allowHoles);
                    if (eooGeometry == null)
                    {
                        return null!;
                    }

                    var area = eooGeometry.Area / 1000000; //Calculate area in km2
                    var eoo = Math.Round(area, 0);
                    var transformedEooGeometry = eooGeometry.Transform((CoordinateSys)metricCoordinateSys, coordinateSystem);
                    
                    futureCollection.Add(new Feature(
                        transformedEooGeometry,
                        new AttributesTable(new KeyValuePair<string, object>[] {
                                new KeyValuePair<string, object>("id", $"eoo-{edgeLength.ToString().Replace(',', '.')}"),
                                new KeyValuePair<string, object>("aoo", (int)aoo),
                                new KeyValuePair<string, object>("eoo", (int)eoo),
                                new KeyValuePair<string, object>("gridCellArea", gridCellArea),
                                new KeyValuePair<string, object>("gridCellAreaUnit", "km2"),
                                new KeyValuePair<string, object>("observationsCount", metaData.ObservationsCount)
                            }
                        )
                    ));
                }
                
                if (!returnGridCells){
                    return futureCollection;
                }

                // Add empty grid cell where no observation was found too complete grid
                if (includeEmptyCells)
                {
                    GeoJsonHelper.FillInBlanks(
                        gridCellFeaturesMetric,
                        new Envelope(
                            gridCellFeaturesMetric.Min(gc => gc.Value.Geometry.Coordinates.Min(c => c.X)),
                            gridCellFeaturesMetric.Max(gc => gc.Value.Geometry.Coordinates.Max(c => c.X)),
                            gridCellFeaturesMetric.Max(gc => gc.Value.Geometry.Coordinates.Max(c => c.Y)),
                            gridCellFeaturesMetric.Min(gc => gc.Value.Geometry.Coordinates.Min(c => c.Y))
                        ),
                        gridCellsInMeters, new[] {
                            new KeyValuePair<string, object>("observationsCount", 0),
                            new KeyValuePair<string, object>("taxaCount", 0)
                        }
                    );
                }

                // Add all grid cells features
                foreach(var gridCellFeatureMetric in gridCellFeaturesMetric.OrderBy(gc => gc.Key))
                {
                    if (coordinateSystem != (CoordinateSys)metricCoordinateSys)
                    {
                        gridCellFeatureMetric.Value.Geometry = gridCellFeatureMetric.Value.Geometry.Transform((CoordinateSys)metricCoordinateSys, coordinateSystem);
                    }
                    
                    futureCollection.Add(gridCellFeatureMetric.Value);
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
    }
}
