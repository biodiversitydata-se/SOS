using NetTopologySuite.Features;
using SOS.Analysis.Api.Dtos.Search;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.Managers.Interfaces
{
    public interface IAnalysisManager
    {
        /// <summary>
        /// Aggregate by user passed field
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedAggregationResultDto<UserAggregationResponseDto>?> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter, 
            string aggregationField,
            string? afterKey, 
            int? take);

        /// <summary>
        /// Aggregate by user passed field
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="take"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<IEnumerable<AggregationItemDto>?> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            int take,
            AggregationSortOrder sortOrder);

        /// <summary>
        /// Calculate AOO and EOO
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="useCenterPoint"></param>
        /// <param name="edgeLengths"></param>
        /// <param name="useEdgeLengthRatio"></param>
        /// <param name="allowHoles"></param>
        /// <param name="returnGridCells"></param>
        /// <param name="includeEmptyCells"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        Task<FeatureCollection> CalculateAooAndEooAsync(
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
            CoordinateSys coordinateSystem);
    }
}
