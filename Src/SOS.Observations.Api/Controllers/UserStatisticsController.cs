﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Models.Statistics;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;
using Result = CSharpFunctionalExtensions.Result;

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
        private readonly ITaxonManager _taxonManager;

        // Observatörsligan och Artlistan för en person är viktigast att uppdatera så snart som möjligt.

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userStatisticsManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UserStatisticsController(
            IUserStatisticsManager userStatisticsManager,
            ITaxonManager taxonManager,
            IAreaManager areaManager,
            ILogger<UserStatisticsController> logger) : base(areaManager)
        {
            _userStatisticsManager = userStatisticsManager ?? throw new ArgumentNullException(nameof(userStatisticsManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Aggregates taxon by user.
        /// </summary>
        /// <param name="skip">Start index of returned records. If null, skip will be set to 0.</param>
        /// <param name="take">Max number of taxa to return. If null, all taxa will be returned. If not null, max number of records is 1000.</param>
        /// <param name="sortBy">Sort by sum or featureId.</param>
        /// <param name="taxonId"></param>
        /// <param name="year"></param>
        /// <param name="speciesGroup"></param>
        /// <param name="areaType"></param>
        /// <param name="featureId"></param>
        /// <param name="siteId"></param>
        /// <param name="includeOtherAreasSpeciesCount">Add user species count for all areas in specified Area Type.</param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        [HttpGet("PagedSpeciesCountAggregation")]
        [ProducesResponseType(typeof(PagedResultDto<UserStatisticsItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> PagedSpeciesCountAggregation(
            [FromQuery] int? skip = null,
            [FromQuery] int? take = null,
            [FromQuery] string sortBy = "sum",
            [FromQuery] int? taxonId = null,
            [FromQuery] int? year = null,
            [FromQuery] SpeciesGroup? speciesGroup = null,
            [FromQuery] AreaType? areaType = null,
            [FromQuery] string featureId = null,
            [FromQuery] int? siteId = null,
            [FromQuery] bool includeOtherAreasSpeciesCount = false,
            [FromQuery] bool useCache = true)
        {
            try
            {
                // todo - add validation
                var query = new SpeciesCountUserStatisticsQuery
                {
                    TaxonId = taxonId,
                    Year = year,
                    SpeciesGroup = speciesGroup,
                    AreaType = areaType,
                    FeatureId = featureId,
                    SiteId = siteId,
                    IncludeOtherAreasSpeciesCount = includeOtherAreasSpeciesCount
                };

                var result = await _userStatisticsManager.PagedSpeciesCountSearchAsync(query, skip, take, sortBy, useCache);
                PagedResultDto<UserStatisticsItem> dto = result.ToPagedResultDto(result.Records);
                return new OkObjectResult(dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "PagedSpeciesCountAggregation error.");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("SpeciesCountAggregation")]
        [ProducesResponseType(typeof(IEnumerable<UserStatisticsItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SpeciesCountAggregation(
            [FromQuery] int? taxonId = null,
            [FromQuery] int? year = null,
            [FromQuery] SpeciesGroup? speciesGroup = null,
            [FromQuery] AreaType? areaType = null,
            [FromQuery] string featureId = null,
            [FromQuery] int? siteId = null,
            [FromQuery] List<int> userIds = null,
            [FromQuery] bool useCache = true)
        {
            try
            {
                // todo - add validation

                var query = new SpeciesCountUserStatisticsQuery
                {
                    TaxonId = taxonId,
                    Year = year,
                    SpeciesGroup = speciesGroup,
                    AreaType = areaType,
                    FeatureId = featureId,
                    SiteId = siteId
                };

                var result = await _userStatisticsManager.SpeciesCountSearchAsync(query, useCache);
                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "SpeciesCountAggregation error.");
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