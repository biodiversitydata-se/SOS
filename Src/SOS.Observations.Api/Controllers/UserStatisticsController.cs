using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Enums;
using SOS.Lib.Models.Statistics;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{ 
    /// <summary>
    ///     User statistics controller.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class UserStatisticsController : SearchBaseController
    {
        private readonly ILogger<UserStatisticsController> _logger;
        private readonly IUserStatisticsManager _userStatisticsManager;

        // Observatörsligan och Artlistan för en person är viktigast att uppdatera så snart som möjligt.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userStatisticsManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserStatisticsController(
            IUserStatisticsManager userStatisticsManager,
            IAreaManager areaManager,
            ObservationApiConfiguration observationApiConfiguration,
            ILogger<UserStatisticsController> logger) : base(areaManager, observationApiConfiguration)
        {
            _userStatisticsManager = userStatisticsManager ?? throw new ArgumentNullException(nameof(userStatisticsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Clear cache.
        /// </summary>
        /// <returns></returns>
        [HttpPost("ClearCache")]
        [ProducesResponseType(typeof(PagedResultDto<UserStatisticsItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ClearCache()
        {
            try
            {
                _userStatisticsManager.ClearCache();
                return new OkObjectResult("Cache cleared");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ClearCache error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregates taxon by user.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of records to return. Max number of records is 100.</param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        [HttpPost("PagedSpeciesCountAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<UserStatisticsItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PagedSpeciesCountAggregation(
            [FromBody] SpeciesCountUserStatisticsQuery query,
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,
            [FromQuery] bool useCache = true)
        {
            try
            {
                // todo - add validation
                var result = await _userStatisticsManager.PagedSpeciesCountSearchAsync(query, skip, take, useCache);
                PagedResultDto<UserStatisticsItem> dto = result.ToPagedResultDto(result.Records);
                return new OkObjectResult(dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "PagedSpeciesCountAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aggregates taxon by user.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of records to return. Max number of records is 100.</param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        [HttpPost("SpeciesCountAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<UserStatisticsItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SpeciesCountAggregation(
            [FromBody] SpeciesCountUserStatisticsQuery query,
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,
            [FromQuery] bool useCache = true)
        {
            try
            {
                // todo - add validation
                var result = await _userStatisticsManager.SpeciesCountSearchAsync(query, skip, take, useCache);
                PagedResultDto<UserStatisticsItem> dto = result.ToPagedResultDto(result.Records);
                return new OkObjectResult(dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SpeciesCountAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Experimental. The functionality should be the same as PagedSpeciesCountAggregation but without the need to create a new index.
        /// Aggregates taxon by user. 
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of records to return. Max number of records is 100.</param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        [HttpPost("ProcessedObservationPagedSpeciesCountAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<UserStatisticsItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ProcessedObservationPagedSpeciesCountAggregation(
            [FromBody] SpeciesCountUserStatisticsQuery query,
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,
            [FromQuery] bool useCache = true)
        {
            try
            {
                // todo - add validation
                Stopwatch sw = Stopwatch.StartNew();
                _logger.LogInformation(
                    $"Start ProcessedObservationPagedSpeciesCountAggregation(skip={skip}, take={take}, useCache={useCache})");
                var result = await _userStatisticsManager.ProcessedObservationPagedSpeciesCountSearchAsync(query, skip, take, useCache);
                PagedResultDto<UserStatisticsItem> dto = result.ToPagedResultDto(result.Records);
                sw.Stop();
                _logger.LogInformation(
                    $"Finish ProcessedObservationPagedSpeciesCountAggregation(skip={skip}, take={take}, useCache={useCache}). Elapsed ms: {sw.ElapsedMilliseconds}");
                return new OkObjectResult(dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "PagedSpeciesCountAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("SpeciesCountByMonthAggregation")]
        [ProducesResponseType(typeof(IEnumerable<UserStatisticsByMonthItem>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SpeciesCountByMonthAggregation(
            [FromQuery] int? taxonId = null,
            [FromQuery] int? year = null,
            [FromQuery] SpeciesGroup? speciesGroup = null)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        [HttpGet("SpeciesCountByYearAndMonthAggregation")]
        [ProducesResponseType(typeof(IEnumerable<UserStatisticsByMonthItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SpeciesCountByYearAndMonthAggregation(
            [FromQuery] int? taxonId = null,
            [FromQuery] int? year = null,
            [FromQuery] SpeciesGroup? speciesGroup = null)
        {
            throw new NotImplementedException("Not implemented yet.");
        }

        [HttpGet("SpeciesListSummary")]
        [ProducesResponseType(typeof(IEnumerable<UserStatisticsByMonthItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SpeciesListSummary(
            [FromQuery] int? taxonId = null,
            [FromQuery] int? year = null,
            [FromQuery] SpeciesGroup? speciesGroup = null,
            [FromQuery] AreaType? areaType = null,
            [FromQuery] string featureId = null,
            [FromQuery] int? siteId = null,
            [FromQuery] string sortBy = "date")
        {
            throw new NotImplementedException("Not implemented yet.");
        }
    }
}