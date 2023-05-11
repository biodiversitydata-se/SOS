using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;

namespace SOS.Analysis.Api.Helpers
{
    public class GeographicsHelper
    {
        public static (Geometry, int, int, int) GetAOOEOO(Polygon[] gridCells, bool useCenterPoint = true, double edgeLength = 0, bool useEdgeLengthRatio = false, bool allowHoles = false)
        {
            if (gridCells == null || !gridCells.Any())
            {
                return (null, 0, 0, 0)!;
            }

            // Transform grid cells to SWEREF99
            var gridCellsSweRef99 = gridCells.Select(gc => gc.Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM) as Polygon).ToArray();
            var eooGeometry = edgeLength == 0 ? gridCellsSweRef99.ConvexHull() : gridCellsSweRef99.ConcaveHull(useCenterPoint, edgeLength, useEdgeLengthRatio, allowHoles);

            if (eooGeometry == null)
            {
                return (null, 0, 0, 0)!;
            }
            
            var gridCellCount = gridCellsSweRef99.Length;
            var firstGrid = gridCellsSweRef99.First();
            var gridCellArea = firstGrid!.Area / 1000000; //Calculate area in km2

            var area = eooGeometry.Area / 1000000; //Calculate area in km2
            var eoo = Math.Round(area, 0);
            var aoo = Math.Round((double)gridCellCount * gridCellArea, 0);

            return (eooGeometry, (int)aoo, (int)eoo, (int)gridCellArea);
        }
    }
}
