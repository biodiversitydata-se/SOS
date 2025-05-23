﻿using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search.Enums;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;

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
        /// <param name="validateFilter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="aggregateOrganismQuantity"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="afterKey"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IActionResult> AggregateAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto searchFilter,
            bool? validateFilter,
            string aggregationField,
            bool? aggregateOrganismQuantity,
            int? precisionThreshold,
            string? afterKey,
            int? take);

        /// <summary>
        ///  Aggregate by user passed filed
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="validateFilter"></param>
        /// <param name="aggregationField"></param>
        /// <param name="aggregateOrganismQuantity"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="take"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        Task<IActionResult> AggregateByUserFieldAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto filter,
            bool? validateFilter,
            string aggregationField,
            bool? aggregateOrganismQuantity,
            int? precisionThreshold,
            int take,
            AggregationSortOrder sortOrder);

        /// <summary>
        /// Area aggregation
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="areaType"></param>
        /// <param name="coordinateSys"></param>
        /// <param name="precisionThreshold"></param>
        /// <param name="aggregateOrganismQuantity"></param>
        /// <param name="validateFilter"></param>
        /// <returns></returns>
        Task<IActionResult> AreaAggregateAsync(
            int? roleId,
            string? authorizationApplicationIdentifier,
            SearchFilterInternalDto searchFilter,
            AreaTypeAggregate areaType,
            CoordinateSys? coordinateSys,
            int? precisionThreshold,
            bool? aggregateOrganismQuantity,
            bool? validateFilter);

        /// <summary>
        /// Calculate AOO and EOO and get geometry showing coverage 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="validateFilter"></param>
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
            bool? validateFilter,
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
        /// <param name="validateFilter"></param>
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
            bool? validateFilter,
            int? maxDistance = 50000,
            int? gridCellSizeInMeters = 2000,
            MetricCoordinateSys? metricCoordinateSys = MetricCoordinateSys.SWEREF99_TM,
            CoordinateSys? coordinateSystem = CoordinateSys.WGS84);
    }
}
