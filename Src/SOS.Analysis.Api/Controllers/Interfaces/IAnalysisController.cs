﻿using Microsoft.AspNetCore.Mvc;
using SOS.Analysis.Api.Dtos.Filter;
using SOS.Lib.Enums;

namespace SOS.Analysis.Api.Controllers.Interfaces
{
    public interface IAnalysisController
    {
        /// <summary>
        /// Calculate AOO and EOO and get geometry showing coverage 
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="searchFilter"></param>
        /// <param name="sensitiveObservations"></param>
        /// <param name="gridCellSizeInMeters">Grid cell size in meters </param>
        /// <param name="useCenterPoint">If true, grid cell center point will be used, else grid cell corner points will be used.</param>
        /// <param name="edgeLength">The target edge length ratio when useEdgeLengthRatio is true, else the target maximum edge length.</param>
        /// <param name="useEdgeLengthRatio">Change behavior of edgeLength. When true: 
        /// Computes the concave hull of the vertices in a geometry using the target criterion of edge length ratio. 
        /// The edge length ratio is a fraction of the length difference between the longest and shortest edges in the Delaunay Triangulation of the input points.
        /// When false: Computes the concave hull of the vertices in a geometry using the target criterion of edge length, and optionally allowing holes (see below). </param>
        /// <param name="allowHoles">Gets or sets whether holes are allowed in the concave hull polygon.</param>
        /// <param name="coordinateSystem">Gemometry coordinate system</param>
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