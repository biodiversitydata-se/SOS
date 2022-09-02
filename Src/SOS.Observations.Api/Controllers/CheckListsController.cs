using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Statistics;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos.Checklist;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Checklists controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ChecklistsController : ControllerBase, IChecklistsController
    {
        private readonly IChecklistManager _checklistManager;
        private readonly ITaxonManager _taxonManager;
        private readonly ILogger<ChecklistsController> _logger;

        private Result ValidateTaxa(IEnumerable<int> taxonIds)
        {
            var missingTaxa = taxonIds?
                .Where(tid => !_taxonManager.TaxonTree.TreeNodeById.ContainsKey(tid))
                .Select(tid => $"TaxonId doesn't exist ({tid})");

            return missingTaxa?.Any() ?? false ?
                Result.Failure(string.Join(". ", missingTaxa))
                :
                Result.Success();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="checklistManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ChecklistsController(
            IChecklistManager checklistManager,
            ITaxonManager taxonManager,
            ILogger<ChecklistsController> logger)
        {
            _checklistManager = checklistManager ?? throw new ArgumentNullException(nameof(checklistManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculate species trend as a quotient. Species trend = (Number of present observations) / (Number of present and absent observations)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("CalculateTrend")]
        [ProducesResponseType(typeof(TaxonTrendResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CalculateTrendAsync([FromBody] CalculateTrendFilterDto filter)
        {
            try
            {
                var taxaValidation = ValidateTaxa(filter?.TaxonId == null ? new int[0] : new[] { filter.TaxonId });

                if (taxaValidation.IsFailure)
                {
                    return BadRequest(taxaValidation.Error);
                }
                var (observationFilter, checklistFilter) = filter.ToSearchFilters();
                var trend = await _checklistManager.CalculateTrendAsync(observationFilter, checklistFilter);

                return new OkObjectResult(trend);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error calculating trend");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get a checklist by eventId.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ChecklistDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChecklistByIdAsync([FromQuery] string id)
        {
            try
            {
                return new OkObjectResult(await _checklistManager.GetChecklistAsync(id));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting checklist");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet("internal")]
        [ProducesResponseType(typeof(ChecklistInternalDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> GetChecklistByIdInternalAsync([FromQuery] string id)
        {
            try
            {
                return new OkObjectResult(await _checklistManager.GetChecklistInternalAsync(id));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting checklist");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
       
    }
}