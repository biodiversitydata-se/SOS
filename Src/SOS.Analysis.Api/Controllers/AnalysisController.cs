﻿using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Features;
using SOS.Analysis.Api.Configuration;
using SOS.Analysis.Api.Controllers.Interfaces;
using SOS.Analysis.Api.Dtos.Filter;
using SOS.Analysis.Api.Extensions.Dto;
using SOS.Analysis.Api.Managers.Interfaces;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Extensions;
using System.Net;
using Result = CSharpFunctionalExtensions.Result;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SOS.Analysis.Api.Controllers
{
    /// <summary>
    /// Implementation of <see cref="IAnalysisController"/>.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : BaseController, IAnalysisController
    {
        private readonly IAnalysisManager _analysisManager;
        private readonly ILogger<AnalysisController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="analysisManager"></param>
        /// <param name="areaCache"></param>
        /// <param name="analysisConfiguration"></param>
        /// <param name="elasticConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnalysisController(
            IAnalysisManager analysisManager,
            IAreaCache areaCache,
            AnalysisConfiguration analysisConfiguration,
            ElasticSearchConfiguration elasticConfiguration,
            ILogger<AnalysisController> logger) : base(areaCache, analysisConfiguration?.ProtectedScope!, elasticConfiguration?.MaxNrAggregationBuckets ?? 0)
        {
            _analysisManager = analysisManager ?? throw new ArgumentNullException(nameof(analysisManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
        [HttpPost("AOO_EOO")]
        [ProducesResponseType(typeof(FeatureCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CalculateAooAndEooAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string? authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto searchFilter,
            [FromQuery] bool? sensitiveObservations = false,
            [FromQuery] int? gridCellSizeInMeters = 2000,
            [FromQuery] bool? useCenterPoint = true,
            [FromQuery] double? edgeLength = 1000,
            [FromQuery] bool? useEdgeLengthRatio = false,
            [FromQuery] bool? allowHoles = false,
            CoordinateSys? coordinateSystem = CoordinateSys.ETRS89)
        {
            try
            {
                CheckAuthorization(sensitiveObservations!.Value);

                searchFilter = await InitializeSearchFilterAsync(searchFilter);
               
                var validationResult = Result.Combine(
                    useEdgeLengthRatio ?? false ? ValidateDouble(edgeLength!.Value, 0.0, 1.0, "Edge length") : Result.Success(),
                    ValidateSearchFilter(searchFilter!),
                    ValidateInt(gridCellSizeInMeters!.Value, minLimit: 100, maxLimit: 100000, "Grid cell size in meters"),
                    ValidateMetricTilesLimit(searchFilter.Geographics!.BoundingBox!.ToEnvelope().Transform(CoordinateSys.WGS84, CoordinateSys.SWEREF99_TM), gridCellSizeInMeters.Value));

                if (validationResult.IsFailure)
                {
                    return BadRequest(validationResult.Error);
                }

                var filter = searchFilter?.ToSearchFilter(UserId, sensitiveObservations.Value, "sv-SE")!;

                var result = await _analysisManager.CalculateAooAndEooAsync(
                    filter, 
                    gridCellSizeInMeters!.Value, 
                    useCenterPoint!.Value, 
                    edgeLength!.Value, 
                    useEdgeLengthRatio!.Value, 
                    allowHoles!.Value, 
                    coordinateSystem!.Value
                );
                return new OkObjectResult(result!);
            }
            catch (AuthenticationRequiredException e)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "GetAooAndEoo error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
