using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Responses;
using SOS.Lib.Models.Shared;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using SOS.Lib.Helpers;
using Microsoft.Net.Http.Headers;
using SOS.Observations.Api.Services;

namespace SOS.Observations.Api.Controllers;

/// <summary>
///     Area controller
/// </summary>
[Route("[controller]")]
[ApiController]
public class AreasController : ControllerBase
{
    private readonly IAreaManager _areaManager;
    private readonly ILogger<AreasController> _logger;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="areaManager"></param>
    /// <param name="logger"></param>
    public AreasController(
        IAreaManager areaManager,
        ILogger<AreasController> logger)
    {
        _areaManager = areaManager ?? throw new ArgumentNullException(nameof(areaManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Search for areas (regions).
    /// </summary>
    /// <param name="areaTypes">Filter used to limit number of areas returned</param>
    /// <param name="searchString">Filter used to limit number of areas returned</param>
    /// <param name="skip">Start index of returned areas</param>
    /// <param name="take">Number of areas to return</param>
    /// <returns>List of areas</returns>
    [HttpGet()]
    [ProducesResponseType(typeof(PagedResultDto<AreaBaseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [AzureApi, AzureInternalApi]
    public async Task<IActionResult> GetAreas(
        [FromQuery] IEnumerable<AreaTypeDto> areaTypes = null,
        [FromQuery] string searchString = null, 
        [FromQuery] int skip = 0, 
        [FromQuery] int take = 100)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var result = await _areaManager.GetAreasAsync(
                areaTypes, searchString, skip, take);
            return new OkObjectResult(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting areas");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get a single area
    /// </summary>
    /// <param name="areaType">The area type.</param>
    /// <param name="featureId">The feature id.</param>
    /// <returns></returns>
    [HttpGet("{areaType}/{featureId}")]
    [ProducesResponseType(typeof(AreaBaseDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [AzureApi, AzureInternalApi]
    public async Task<IActionResult> GetArea(
        [FromRoute] AreaTypeDto areaType,
        [FromRoute] string featureId)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var result = await _areaManager.GetAreaAsync(areaType, featureId);
            return new OkObjectResult(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting area {@areaType} {@featureId}", areaType, featureId);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get an area's complete geometry as a WGS84 GeoJSON Feature. The response is cacheable for one hour.
    /// Retain the ETag and send If-None-Match to revalidate; an unchanged geometry returns 304 without a body.
    /// </summary>
    /// <param name="areaType">The area type from a filterAreaReferences entry.</param>
    /// <param name="featureId">The feature identifier from a filterAreaReferences entry.</param>
    /// <param name="geometryService">Geometry serialization provider.</param>
    [HttpGet("{areaType}/{featureId}/geometry")]
    [Produces("application/geo+json")]
    [ProducesResponseType(typeof(GeoJSON.Text.Feature.Feature), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetGeometry(
        [FromRoute] AreaTypeDto areaType,
        [FromRoute] string featureId,
        [FromServices] IAreaGeometryService geometryService)
    {
        if (!Enum.IsDefined(areaType) || string.IsNullOrWhiteSpace(featureId))
            return BadRequest("A valid areaType and non-empty featureId are required.");

        try
        {
            var geometry = await geometryService.GetAsync(areaType, featureId, HttpContext.RequestAborted);
            if (geometry == null) return NotFound();

            Response.Headers.CacheControl = AreaGeometryCacheDefaults.CacheControl;
            Response.Headers.ETag = geometry.ETag;
            var validator = EntityTagHeaderValue.Parse(geometry.ETag);
            if (EntityTagHeaderValue.TryParseList(Request.Headers.IfNoneMatch.ToArray(), out var validators) &&
                validators.Any(value => value.Tag == "*" || value.Tag.Equals(validator.Tag)))
            {
                return StatusCode((int)HttpStatusCode.NotModified);
            }

            return File(geometry.Content, "application/geo+json; charset=utf-8");
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            return new EmptyResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting area geometry {AreaType} {FeatureId}", areaType, featureId);
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    ///     Get an area as a zipped JSON file including its polygon.
    /// </summary>
    /// <param name="areaType">The area type.</param>
    /// <param name="featureId">The FeatureId.</param>
    /// <param name="format">Export format.</param>
    /// <returns></returns>
    [HttpGet("{areaType}/{featureId}/Export")]
    [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [AzureApi, AzureInternalApi]
    public async Task<IActionResult> GetExport(
        [FromRoute] AreaTypeDto areaType,
        [FromRoute] string featureId,
        [FromQuery] AreaExportFormatDto format = AreaExportFormatDto.Json)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var zipBytes = await _areaManager.GetZippedAreaAsync(areaType, featureId, (AreaExportFormat)format);

            if (zipBytes == null)
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }

            return File(zipBytes, "application/zip", $"Area{areaType:G}:{featureId}.zip");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting areas");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}
