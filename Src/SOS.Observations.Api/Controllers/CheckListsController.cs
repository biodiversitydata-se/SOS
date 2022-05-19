using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.CheckList;
using SOS.Observations.Api.Controllers.Interfaces;
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
    public class ChecklistsController : ControllerBase, ICheckListsController
    {
        private readonly ICheckListManager _checkListManager;
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
        /// <param name="checkListManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        public ChecklistsController(
            ICheckListManager checkListManager,
            ITaxonManager taxonManager,
            ILogger<ChecklistsController> logger)
        {
            _checkListManager = checkListManager ?? throw new ArgumentNullException(nameof(checkListManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculate species trend as a quotient. Species trend = (Number of present observations) / (Number of present and absent observations)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("CalculateTrend")]
        [ProducesResponseType(typeof(double), (int)HttpStatusCode.OK)]
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
                var (observationFilter, checkListFilter) = filter.ToSearchFilters();
                var trend = await _checkListManager.CalculateTrendAsync(observationFilter, checkListFilter);

                return new OkObjectResult(trend);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error calculating trend");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get a check list by eventId.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(CheckList), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetChecklistByIdAsync([FromQuery] string id)
        {
            try
            {
                return new OkObjectResult(await _checkListManager.GetCheckListAsync(id));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting check list");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}