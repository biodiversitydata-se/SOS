using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers.V2
{
    /// <summary>
    ///     Observation controller
    /// </summary>
    [ApiVersion("2.0")]
    [Route("[controller]")]
    [ApiController]
    public class ObservationsController : ControllerBase
    {
        private const int MaxBatchSize = 1000;
        private const int ElasticSearchMaxRecords = 10000;
        private readonly IFieldMappingManager _fieldMappingManager;
        private readonly ILogger<ObservationsController> _logger;
        private readonly IObservationManager _observationManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="fieldMappingManager"></param>
        /// <param name="logger"></param>
        public ObservationsController(
            IObservationManager observationManager,
            IFieldMappingManager fieldMappingManager,
            ILogger<ObservationsController> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _fieldMappingManager = fieldMappingManager ?? throw new ArgumentNullException(nameof(fieldMappingManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Search for observations.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sortBy"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is an example of how to handle observation search API versioning.
        /// There are two difference compared to API v1.
        /// 1. OutputFields are mapped to new property names.
        /// 2. The dynamic observation model (due to projection) is converted to ObservationV2 property names.
        ///
        /// What is excluded in this sample?
        /// The response is of type ProcessedObservation, i.e. the same as V1. Either the ObservationDtoV2 should be
        /// the response type (=> add properties and documentation to that model) or add another swagger attribute to the changed
        /// properties that is read when created the swagger documentation.
        /// </remarks>
        [HttpPost("search")]
        [ProducesResponseType(typeof(PagedResultDto<Observation>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetObservationsAsync(
            [FromBody] SearchFilterDto filter,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100,
            [FromQuery] string sortBy = "",
            [FromQuery] SearchSortOrder sortOrder = SearchSortOrder.Asc)
        {
            try
            {
                var pagingArgumentsValidation = ValidateSearchPagingArguments(skip, take);
                var searchFilterValidation = ValidateSearchFilter(filter);
                var validationResult = Result.Combine(pagingArgumentsValidation, searchFilterValidation);
                if (validationResult.IsFailure) return BadRequest(validationResult.Error);

                var changedPropertyNames = new Dictionary<string, string>
                {
                    { "lang", "language" }
                };
                SearchFilter searchFilter = filter.ToSearchFilter();
                if (searchFilter.OutputFields != null)
                {
                    var outputFields = searchFilter.OutputFields.ToList();
                    for (var i = 0; i < outputFields.Count; i++)
                    {
                        if (changedPropertyNames.TryGetValue(outputFields[i].ToLower(), out var value))
                        {
                            outputFields[i] = value;
                        }
                    }

                    searchFilter.OutputFields = outputFields;
                }

                var result = await _observationManager.GetChunkAsync(searchFilter, skip, take, sortBy, sortOrder);
                PagedResultDto<dynamic> dto = result.ToPagedResultDto(result.Records.Select(m => ObservationDtoV2.TransformToV2(m)));
                return new OkObjectResult(dto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting batch of sightings");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        private Result ValidateSearchFilter(SearchFilterDto filter)
        {
            var errors = new List<string>();

            // No culture code, set default
            if (string.IsNullOrEmpty(filter?.TranslationCultureCode))
            {
                filter.TranslationCultureCode = "sv-SE";
            }

            if (!new[] { "sv-SE", "en-GB" }.Contains(filter.TranslationCultureCode,
                StringComparer.CurrentCultureIgnoreCase))
            {
                errors.Add("Unknown FieldTranslationCultureCode. Supported culture codes, sv-SE, en-GB");
            }

            if (errors.Count > 0) return Result.Failure(string.Join(". ", errors));
            return Result.Success();
        }

        private Result ValidateSearchPagingArguments(int skip, int take)
        {
            var errors = new List<string>();

            //Remove the limitations if we use the internal functions
            if (skip < 0 || take <= 0 || take > MaxBatchSize)
            {
                errors.Add($"You can't take more than {MaxBatchSize} at a time.");
            }

            if (skip + take > ElasticSearchMaxRecords)
            {
                errors.Add($"Skip + take can't be greater than { ElasticSearchMaxRecords }");
            }

            if (errors.Count > 0) return Result.Failure(string.Join(". ", errors));
            return Result.Success();
        }
    }
}