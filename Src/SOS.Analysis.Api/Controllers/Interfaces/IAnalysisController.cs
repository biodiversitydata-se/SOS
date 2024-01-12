﻿using Microsoft.AspNetCore.Mvc;
using SOS.Analysis.Api.Dtos.Enums;
using SOS.Analysis.Api.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Enums;

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
        /// <param name="precisionThreshold"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IActionResult> AggregateAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto searchFilter,
            string aggregationField,
            int? precisionThreshold,
            string? afterKey,
            int? take);

        /// <summary>
        ///  Aggregate by user passed filed
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="take"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<IActionResult> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto filter,
            string aggregationField,
            int? precisionThreshold,
            int take,
            AggregationSortOrder sortOrder);

        /// <summary>
        /// Atlas square aggregation
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="atlasSize"></param>
        /// <returns></returns>
        Task<IActionResult> AtlasAggregateAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto searchFilter,
            AtlasAreaSizeDto atlasSize = AtlasAreaSizeDto.Km10x10);

        /// <summary>
        /// Calculate AOO and EOO and get geometry showing coverage 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="alphaValues">One or more alpha values used to calculate AOO and EEO</param>
        /// <param name="gridCellSizeInMeters">Grid cell size in meters </param>
        /// <param name="useCenterPoint">If true, grid cell center point will be used, else grid cell corner points will be used.</param>
        /// <param name="useEdgeLengthRatio">Change behavior of alpha values. When true: Computes the concave hull of the vertices in a geometry using the target criterion of maximum alpha ratio. 
        /// The alpha factor is a fraction of the length difference between the longest and shortest edges in the Delaunay Triangulation of the input points.
        /// When false: 
        /// Computes the concave hull of the vertices in a geometry using the target criterion of maximum edge length.</param>
        /// <param name="allowHoles">Gets or sets whether holes are allowed in the concave hull polygon.</param>
        /// <param name="returnGridCells">Return grid cells features</param>
        /// <param name="includeEmptyCells">Include grid cells with no observations</param>
        /// <param name="metricCoordinateSys">Coordinate system used to calculate the grid</param>
        /// <param name="coordinateSystem">Gemometry coordinate system
        /// Computes the concave hull of the vertices in a geometry using the target criterion of maximum alpha ratio. 
        /// The alpha factor is a fraction of the length difference between the longest and shortest edges in the Delaunay Triangulation of the input points.
        /// When false: 
        /// Computes the concave hull of the vertices in a geometry using the target criterion of maximum edge length.</param>
        /// <returns></returns>
        Task<IActionResult> CalculateAooAndEooInternalAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto searchFilter,
            double[] alphaValues,
            int? gridCellSizeInMeters = 2000,
            bool? useCenterPoint = true,
            bool? useEdgeLengthRatio = true,
            bool? allowHoles = false,
            bool? returnGridCells = false,
            bool? includeEmptyCells = false,
            MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            CoordinateSys? coordinateSystem = CoordinateSys.WGS84);

        /// <summary>
        /// Calculate AOO and EOO and get geometry showing coverage 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="maxDistance">Max distance between occurrence grid cells</param>
        /// <param name="gridCellSizeInMeters">Grid cell size in meters </param>
        /// <param name="metricCoordinateSys">Coordinate system used to calculate the grid</param>
        /// <param name="coordinateSystem">Gemometry coordinate system
        /// Computes the concave hull of the vertices in a geometry using the target criterion of maximum alpha ratio. 
        /// The alpha factor is a fraction of the length difference between the longest and shortest edges in the Delaunay Triangulation of the input points.
        /// When false: 
        /// Computes the concave hull of the vertices in a geometry using the target criterion of maximum edge length.</param>
        /// <returns></returns>
        Task<IActionResult> CalculateAooAndEooArticle17InternalAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto searchFilter,
            int maxDistance,
            int? gridCellSizeInMeters = 2000,
            MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            CoordinateSys? coordinateSystem = CoordinateSys.WGS84);
    }
}
