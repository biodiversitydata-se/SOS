using Microsoft.AspNetCore.Mvc;
using SOS.Analysis.Api.Dtos.Filter;
using SOS.Lib.Enums;

namespace SOS.Analysis.Api.Controllers.Interfaces
{
    public interface IAnalysisController
    {
        /// <summary>
        /// Calculate AOO and EOO and get map layer representing it
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="sensitiveObservations"></param>
        /// <param name="gridCellSizeInMeters"></param>
        /// <param name="useCenterPoint"></param>
        /// <param name="edgeLength"></param>
        /// <param name="useEdgeLengthRatio"></param>
        /// <param name="allowHoles"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        Task<IActionResult> CalculateAooAndEooAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterDto searchFilter,
            bool? sensitiveObservations = false,
            int? gridCellSizeInMeters = 2000,
            bool? useCenterPoint = true, 
            double? edgeLength = 1000, 
            bool? useEdgeLengthRatio = false, 
            bool? allowHoles = false,
            CoordinateSys? coordinateSystem = CoordinateSys.ETRS89);
    }
}
