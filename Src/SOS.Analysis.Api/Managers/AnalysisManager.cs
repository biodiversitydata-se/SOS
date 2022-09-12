using NetTopologySuite.Features;
using SOS.Analysis.Api.Managers.Interfaces;
using SOS.Analysis.Api.Repositories.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.Managers
{
    public class AnalysisManager : IAnalysisManager
    {
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ILogger<AnalysisManager> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnalysisManager(
            IProcessedObservationRepository processedObservationRepository,
            ILogger<AnalysisManager> logger)
        {
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<FeatureCollection> CalculateAooAndEooAsync(
             SearchFilter filter,
             int gridCellsInMeters,
             bool useCenterPoint,
             double edgeLength,
             bool useEdgeLengthRatio,
             bool allowHoles,
             CoordinateSys coordinateSystem)
        {

            try
            {
                var result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters);
                if (!result?.GridCells?.Any() ?? true)
                {
                    return null!;
                }

                var gridCellsSweRef99 = result?.GridCells.Select(gc => gc.Sweref99TmBoundingBox.ToPolygon()).ToArray();
                var eooGeometry = edgeLength == 0 ? gridCellsSweRef99.ConvexHull() : gridCellsSweRef99.ConcaveHull(useCenterPoint, edgeLength, useEdgeLengthRatio, allowHoles);

                if (eooGeometry == null)
                {
                    return null!;
                }

                var gridCellCount = gridCellsSweRef99.Length;
                var firstGrid = gridCellsSweRef99.First();
                var gridCellArea = firstGrid!.Area / 1000000; //Calculate area in km2

                var area = eooGeometry.Area / 1000000; //Calculate area in km2
                var eoo = Math.Round(area, 0);
                var aoo = Math.Round((double)gridCellCount * gridCellArea, 0);

                var futureCollection = new FeatureCollection();
                futureCollection.Add(new Feature(
                    eooGeometry.Transform(CoordinateSys.SWEREF99_TM, coordinateSystem),
                    new AttributesTable(new KeyValuePair<string, object>[] {
                            new KeyValuePair<string, object>("aoo", (int)aoo),
                            new KeyValuePair<string, object>("eoo", (int)eoo),
                            new KeyValuePair<string, object>("gridCellArea", gridCellArea)
                        }
                    )
                ));

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
