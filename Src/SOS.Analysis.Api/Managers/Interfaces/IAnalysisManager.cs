using NetTopologySuite.Features;
using SOS.Analysis.Api.Dtos.Enums;
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
        /// <param name="precisionThreshold"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<PagedAggregationResultDto<UserAggregationResponseDto>?> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            string? afterKey,
            int? take);

        /// <summary>
        /// Aggregate by user passed field
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="take"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<IEnumerable<AggregationItemDto>?> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            int take,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending);

        /// <summary>
        /// Aggregate by atlas square
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="atlasSize"></param>
        /// <returns></returns>
        Task<FeatureCollection> AtlasAggregateAsync(
        int? roleId,
        string? authorizationApplicationIdentifier,
        SearchFilter filter,
        AtlasAreaSizeDto atlasSize);

        /// <summary>
        /// Calculate AOO and EOO
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="useCenterPoint"></param>
        /// <param name="alphaValues"></param>
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
            IEnumerable<double> alphaValues,
            bool useEdgeLengthRatio,
            bool allowHoles,
            bool returnGridCells,
            bool includeEmptyCells,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem);

        /// <summary>
        /// Calculate AOO EOO for Article 17
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="maxDistance"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        Task<FeatureCollection> CalculateAooAndEooArticle17Async(
           int? roleId,
           string? authorizationApplicationIdentifier,
           SearchFilter filter,
           int gridCellsInMeters,
           int maxDistance,
           MetricCoordinateSys metricCoordinateSys,
           CoordinateSys coordinateSystem);
        
        /// <summary>
        /// Get count of observations matching search criteria
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<long> GetMatchCountAsync(int? roleId, string? authorizationApplicationIdentifier, SearchFilterBase filter);
    }
}
