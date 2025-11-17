using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Shared.Api.Dtos.DataStewardship;
using SOS.Shared.Api.Dtos.DataStewardship.Enums;
using SOS.Shared.Api.Dtos.DataStewardship.Extensions;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Extensions.Dto;
using SOS.Shared.Api.Validators.Interfaces;
using SOS.Shared.Api.Extensions.Controller;
using Result = CSharpFunctionalExtensions.Result;
using System;
using System.Net;
using System.Threading.Tasks;
using SOS.Lib.Helpers;
using SOS.Shared.Api.Dtos;

namespace SOS.Observations.Api.Controllers;

/// <summary>
///     Observation controller
/// </summary>
[Route("[controller]")]
[ApiController]
public class DataStewardshipController : ControllerBase
{
    private readonly IDataStewardshipManager _dataStewardshipManager;
    private readonly IInputValidator _inputValidator;
    private readonly ILogger<DataStewardshipController> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dataStewardshipManager"></param>
    /// <param name="inputValidator"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DataStewardshipController(
        IDataStewardshipManager dataStewardshipManager,
        IInputValidator inputValidator,
        ILogger<DataStewardshipController> logger) 
    {
        _dataStewardshipManager = dataStewardshipManager ?? throw new ArgumentNullException(nameof(dataStewardshipManager));
        _inputValidator = inputValidator ?? throw new ArgumentNullException(nameof(inputValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get a single dataset by id
    /// </summary>
    /// <param name="roleId">Limit user authorization too specified role.</param>
    /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
    /// <param name="id">The dataset id.</param>
    /// <param name="exportMode">The export mode.</param>
    /// <returns></returns>
    [HttpGet("datasets/{id}")]
    [ProducesResponseType(typeof(DsDatasetDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> GetDatasetByIdAsync(
        [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
        [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
        [FromRoute] string id,
        [FromQuery] DsExportMode exportMode = DsExportMode.Json
    )
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var dataset = await _dataStewardshipManager.GetDatasetByIdAsync(id);
            if (dataset == null) return new StatusCodeResult((int)HttpStatusCode.NoContent);

            return exportMode.Equals(DsExportMode.Csv) ?
                File(dataset.ToCsv(), "text/tab-separated-values", "dataset.csv") :
                new OkObjectResult(dataset);
        }
        catch (AuthenticationRequiredException e)
        {
            _logger.LogInformation(e, e.Message);
            _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
            _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
        catch (TimeoutException)
        {
            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Get dataset by id ({@datasetId}) failed", id);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Search for datasets
    /// </summary>
    /// <param name="roleId">Limit user authorization too specified role.</param>
    /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
    /// <param name="filter">The search filter.</param>
    /// <param name="includeEventIds">Include list of event id's if true</param>
    /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
    /// <param name="skip">Pagination start index.</param>
    /// <param name="take">Number of items to return.</param>
    /// <param name="exportMode">The export mode.</param>
    /// <returns></returns>
    [HttpPost("datasets")]
    [ProducesResponseType(typeof(PagedResultDto<DsDatasetDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> GetDatasetsBySearchAsync(
        [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
        [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
        [FromBody] SearchFilterBaseDto filter,
        [FromQuery] bool includeEventIds = false,
        [FromQuery] bool validateSearchFilter = false,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        [FromQuery] DsExportMode exportMode = DsExportMode.Json
    )
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var validationResult = Result.Combine(
                _inputValidator.ValidateSearchPagingArguments(skip, take),
                validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false));
            
            if (validationResult.IsFailure)
            {
                return BadRequest(validationResult.Error);
            }
            
            var searchFilter = filter.ToSearchFilter(this.GetUserId(), ProtectionFilterDto.Public, "sv-SE");
            if (!includeEventIds)
            {
                searchFilter ??= new Lib.Models.Search.Filters.SearchFilter();
                searchFilter.Output.ExcludeFields.Add("eventIds");
            }

            var result = await _dataStewardshipManager.GetDatasetsBySearchAsync(searchFilter, skip, take);

            return exportMode.Equals(DsExportMode.Csv) ?
                File(result.Records.ToCsv(), "text/tab-separated-values", "dataset.csv") :
                new OkObjectResult(result);
        }
        catch (AuthenticationRequiredException e)
        {
            _logger.LogInformation(e, e.Message);
            _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
            _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
        catch (TimeoutException)
        {
            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Dataset search failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get a single event by id.
    /// </summary>
    /// <param name="roleId">Limit user authorization too specified role.</param>
    /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
    /// <param name="id">The event ID.</param>
    /// <param name="responseCoordinateSystem"></param>
    /// <param name="exportMode">The export mode.</param>
    /// <returns></returns>
    [HttpGet("events/{id}")]
    [ProducesResponseType(typeof(DsEventDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)] 
    [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> GetEventByIdAsync(
        [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
        [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
        [FromRoute] string id,
        [FromQuery] CoordinateSys responseCoordinateSystem = CoordinateSys.WGS84,
        [FromQuery] DsExportMode exportMode = DsExportMode.Json
    )
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var @event = await _dataStewardshipManager.GetEventByIdAsync(id, responseCoordinateSystem);
            if (@event == null) return new StatusCodeResult((int)HttpStatusCode.NoContent);

            return exportMode.Equals(DsExportMode.Csv) ?
                File(@event.ToCsv(), "text/tab-separated-values", "event.csv") :
                new OkObjectResult(@event);
        }
        catch (AuthenticationRequiredException e)
        {
            _logger.LogInformation(e, e.Message);
            _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
            _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
        catch (TimeoutException)
        {
            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Get event by id ({@eventId}) failed", id);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Search for events
    /// </summary>
    /// <param name="roleId">Limit user authorization too specified role.</param>
    /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
    /// <param name="filter">The search filter.</param>
    /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
    /// <param name="skip">Pagination start index.</param>
    /// <param name="take">Number of items to return.</param>
    /// <param name="responseCoordinateSystem"></param>
    /// <param name="exportMode">The export mode.</param>
    /// <returns></returns>
    [HttpPost("events")]
    [ProducesResponseType(typeof(PagedResultDto<DsEventDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> GetEventsBySearchAsync(
        [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
        [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
        [FromBody] SearchFilterBaseDto filter,
        [FromQuery] bool validateSearchFilter = false,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        [FromQuery] CoordinateSys responseCoordinateSystem = CoordinateSys.WGS84,
        [FromQuery] DsExportMode exportMode = DsExportMode.Json
    )
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var validationResult = Result.Combine(
                _inputValidator.ValidateSearchPagingArguments(skip, take),
                validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false));

            if (validationResult.IsFailure)
            {
                return BadRequest(validationResult.Error);
            }
            var searchFilter = filter.ToSearchFilter(this.GetUserId(), ProtectionFilterDto.Public, "sv-SE");

            var result = await _dataStewardshipManager.GetEventsBySearchAsync(searchFilter, skip, take, responseCoordinateSystem);

            return exportMode.Equals(DsExportMode.Csv) ?
                File(result.Records.ToCsv(), "text/tab-separated-values", "events.csv") :
                new OkObjectResult(result);
        }
        catch (AuthenticationRequiredException e)
        {
            _logger.LogInformation(e, e.Message);
            _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
            _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
        catch (TimeoutException)
        {
            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Event search failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Get a occurrence by id
    /// </summary>
    /// <param name="roleId">Limit user authorization too specified role.</param>
    /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
    /// <param name="id">The occurrence id.</param>
    /// <param name="responseCoordinateSystem"></param>
    /// <param name="exportMode">The export mode.</param>
    /// <returns></returns>
    [HttpGet("occurrences/{id}")]
    [ProducesResponseType(typeof(DsOccurrenceDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)] 
    [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> GetOccurrenceByIdAsync(
        [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
        [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
        [FromRoute] string id,
        [FromQuery] CoordinateSys responseCoordinateSystem = CoordinateSys.WGS84,
        [FromQuery] DsExportMode exportMode = DsExportMode.Json
    )
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var occurrence = await _dataStewardshipManager.GetOccurrenceByIdAsync(id, responseCoordinateSystem);
            if (occurrence == null) return new StatusCodeResult((int)HttpStatusCode.NoContent);

            return exportMode.Equals(DsExportMode.Csv) ?
                File(occurrence.ToCsv(), "text/tab-separated-values", "event.csv") :
                new OkObjectResult(occurrence);
        }
        catch (AuthenticationRequiredException e)
        {
            _logger.LogInformation(e, e.Message);
            _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
            _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
        catch (TimeoutException)
        {
            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Get occurrence by id ({@occurrenceId}) failed", id);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Search for occurrences
    /// </summary>
    /// <param name="roleId">Limit user authorization too specified role.</param>
    /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
    /// <param name="filter">The search filter.</param>
    /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
    /// <param name="skip">Pagination start index.</param>
    /// <param name="take">Number of items to return.</param>
    /// <param name="responseCoordinateSystem"></param>
    /// <param name="exportMode">The export mode.</param>
    /// <returns></returns>
    [HttpPost("occurrences")]
    [ProducesResponseType(typeof(PagedResultDto<DsOccurrenceDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
    public async Task<IActionResult> GetOccurrencesBySearchAsync(
        [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
        [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
        [FromBody] SearchFilterBaseDto filter,
        [FromQuery] bool validateSearchFilter = false,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        [FromQuery] CoordinateSys responseCoordinateSystem = CoordinateSys.WGS84,
        [FromQuery] DsExportMode exportMode = DsExportMode.Json
    )
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var validationResult = Result.Combine(
                _inputValidator.ValidateSearchPagingArguments(skip, take),
                validateSearchFilter ? (await _inputValidator.ValidateSearchFilterAsync(filter)) : Result.Success(),
                _inputValidator.ValidateBoundingBox(filter?.Geographics?.BoundingBox, false));

            if (validationResult.IsFailure)
            {
                return BadRequest(validationResult.Error);
            }
            var searchFilter = filter.ToSearchFilter(this.GetUserId(), ProtectionFilterDto.Public, "sv-SE");

            var result = await _dataStewardshipManager.GetOccurrencesBySearchAsync(searchFilter, skip, take, responseCoordinateSystem);

            return exportMode.Equals(DsExportMode.Csv) ?
                File(result.Records.ToCsv(), "text/tab-separated-values", "occurrences.csv") :
                new OkObjectResult(result);
        }
        catch (AuthenticationRequiredException e)
        {
            _logger.LogInformation(e, e.Message);
            _logger.LogInformation($"Unauthorized. X-Authorization-Application-Identifier={authorizationApplicationIdentifier ?? "[null]"}");
            _logger.LogInformation($"Unauthorized. X-Authorization-Role-Id={roleId?.ToString() ?? "[null]"}");
            return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
        }
        catch (TimeoutException)
        {
            return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Event search failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}