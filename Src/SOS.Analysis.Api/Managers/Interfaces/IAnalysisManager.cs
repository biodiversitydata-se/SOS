using NetTopologySuite.Features;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.Managers.Interfaces
{
    public interface IAnalysisManager
    {
        /// <summary>
        /// Calculate AOO and EOO
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="useCenterPoint"></param>
        /// <param name="edgeLength"></param>
        /// <param name="useEdgeLengthRatio"></param>
        /// <param name="allowHoles"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        Task<FeatureCollection> CalculateAooAndEooAsync(
              SearchFilter filter,
              int gridCellsInMeters,
              bool useCenterPoint,
              double edgeLength,
              bool useEdgeLengthRatio,
              bool allowHoles,
              CoordinateSys coordinateSystem);
    }
}
