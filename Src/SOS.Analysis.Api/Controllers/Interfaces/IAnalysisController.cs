using Microsoft.AspNetCore.Mvc;
using SOS.Analysis.Api.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;

namespace SOS.Analysis.Api.Controllers.Interfaces
{
    public interface IAnalysisController
    {
        /// <summary>
        ///  Aggregate by user passed filed, paging functionality
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IActionResult> AggregateAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto searchFilter,
            string aggregationField,
            string? afterKey,
            int? take);

        /// <summary>
        ///  Aggregate by user passed filed
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="take"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<IActionResult> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto filter,
            string aggregationField,
            int take,
            AggregationSortOrder sortOrder);

        /// <summary>
        /// Calculate AOO and EOO and get geometry showing coverage 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="edgeLengths">One or more edge lenghts to calculate AOO and EEO</param>
        /// <param name="gridCellSizeInMeters">Grid cell size in meters </param>
        /// <param name="useCenterPoint">If true, grid cell center point will be used, else grid cell corner points will be used.</param>
        /// <param name="useEdgeLengthRatio">Change behavior of edgeLength. When true: 
        /// Computes the concave hull of the vertices in a geometry using the target criterion of edge length ratio. 
        /// The edge length ratio is a fraction of the length difference between the longest and shortest edges in the Delaunay Triangulation of the input points.
        /// When false: Computes the concave hull of the vertices in a geometry using the target criterion of edge length, and optionally allowing holes (see below). </param>
        /// <param name="allowHoles">Gets or sets whether holes are allowed in the concave hull polygon.</param>
        /// <param name="returnGridCells">Return grid cells features</param>
        /// <param name="includeEmptyCells">Include grid cells with no observations</param>
        /// <param name="metricCoordinateSys">Coordinate system used to calculate the grid</param>
        /// <param name="coordinateSystem">Gemometry coordinate system</param>
        /// <returns></returns>
        Task<IActionResult> CalculateAooAndEooInternalAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto searchFilter,
            double[] edgeLengths,
            int? gridCellSizeInMeters = 2000,
            bool? useCenterPoint = true,
            bool? useEdgeLengthRatio = true, 
            bool? allowHoles = false,
            bool? returnGridCells = false,
            bool? includeEmptyCells = false,
            MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            CoordinateSys? coordinateSystem = CoordinateSys.WGS84);
    }
}
