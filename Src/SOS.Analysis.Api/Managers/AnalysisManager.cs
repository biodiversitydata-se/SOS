using AgileObjects.AgileMapper.Extensions;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Analysis.Api.Dtos.Search;
using SOS.Analysis.Api.Managers.Interfaces;
using SOS.Analysis.Api.Repositories.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.Managers
{
    public class AnalysisManager : IAnalysisManager
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IFilterManager _filterManager;
        private readonly ILogger<AnalysisManager> _logger;

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
            double edgeLength,
            bool useEdgeLengthRatio,
            bool allowHoles,
            bool includeEmptyCells,
            CoordinateSys coordinateSystem)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
                var result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters);
                if (!result?.GridCells?.Any() ?? true)
                {
                    return null!;
                }

                // We need features to return later so we create them now 
                var gridCellFeaturesSweRef99 = result!.GridCells.Select(gc => gc.Sweref99TmBoundingBox
                    .ToPolygon()
                    .ToFeature(new Dictionary<string, object>()
                    {
                        {  "id", GeoJsonHelper.GetGridCellId(gridCellsInMeters, (int)gc.Sweref99TmBoundingBox.TopLeft.X, (int)gc.Sweref99TmBoundingBox.BottomRight.Y) },
                        {  "observationsCount", gc.ObservationsCount! },
                        {  "taxaCount", gc.TaxaCount! }
                    })
                ).ToDictionary(f => (string)f.Attributes["id"], f => f);

                var eooGeometry = gridCellFeaturesSweRef99.Select(f => f.Value.Geometry as Polygon).ToArray().ConcaveHull(useCenterPoint, edgeLength, useEdgeLengthRatio, allowHoles);

                if (eooGeometry == null)
                {
                    return null!;
                }

                // Add empty grid cell where no observation was found too complete grid
                if (includeEmptyCells)
                {
                    GeoJsonHelper.FillInBlanks(
                        gridCellFeaturesSweRef99,
                        new Envelope(
                            gridCellFeaturesSweRef99.Min(gc => gc.Value.Geometry.Coordinates.Min(c => c.X)),
                            gridCellFeaturesSweRef99.Max(gc => gc.Value.Geometry.Coordinates.Max(c => c.X)),
                            gridCellFeaturesSweRef99.Max(gc => gc.Value.Geometry.Coordinates.Max(c => c.Y)),
                            gridCellFeaturesSweRef99.Min(gc => gc.Value.Geometry.Coordinates.Min(c => c.Y))
                        ),
                        gridCellsInMeters, new[] {
                            new KeyValuePair<string, object>("observationsCount", 0),
                            new KeyValuePair<string, object>("taxaCount", 0)
                        }
                    );
                }

                var area = eooGeometry.Area / 1000000; //Calculate area in km2
                var eoo = Math.Round(area, 0);
                var gridCellCount = result.GridCells.Count();
                var gridCellArea = gridCellsInMeters * gridCellsInMeters / 1000000; //Calculate area in km2
                var aoo = Math.Round((double)gridCellCount * gridCellArea, 0);
                var transformedEooGeometry = eooGeometry.Transform(CoordinateSys.SWEREF99_TM, coordinateSystem);

                var futureCollection = new FeatureCollection() { BoundingBox = transformedEooGeometry?.Envelope.ToEnvelope() };
                futureCollection.Add(new Feature(
                    transformedEooGeometry,
                    new AttributesTable(new KeyValuePair<string, object>[] {
                            new KeyValuePair<string, object>("id", "eoo"),
                            new KeyValuePair<string, object>("aoo", (int)aoo),
                            new KeyValuePair<string, object>("eoo", (int)eoo),
                            new KeyValuePair<string, object>("gridCellArea", gridCellArea),
                            new KeyValuePair<string, object>("gridCellAreaUnit", "km2")
                        }
                    )
                ));

                // Add all grid cells features
                foreach(var gridCellFeatureSweRef99 in gridCellFeaturesSweRef99.OrderBy(gc => gc.Key))
                {
                    if (coordinateSystem != CoordinateSys.SWEREF99_TM)
                    {
                        gridCellFeatureSweRef99.Value.Geometry = gridCellFeatureSweRef99.Value.Geometry.Transform(CoordinateSys.SWEREF99_TM, coordinateSystem);
                    }
                    
                    futureCollection.Add(gridCellFeatureSweRef99.Value);
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
