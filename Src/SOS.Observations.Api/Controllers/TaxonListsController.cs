using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Helpers;
using SOS.Lib.Swagger;
using SOS.Shared.Api.Dtos;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Shared.Api.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Taxon lists controller
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class TaxonListsController : ControllerBase
    {
        private readonly ITaxonListManager _taxonListManager;
        private readonly IEnumerable<int> _signalSearchTaxonListIds;
        private readonly ILogger<TaxonListsController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonListManager"></param>
        /// <param name="inputValaidationConfiguration"></param>
        /// <param name="logger"></param>
        public TaxonListsController(
            ITaxonListManager taxonListManager,
            InputValaidationConfiguration inputValaidationConfiguration,
            ILogger<TaxonListsController> logger)
        {
            _taxonListManager = taxonListManager ?? throw new ArgumentNullException(nameof(taxonListManager));
            _signalSearchTaxonListIds = inputValaidationConfiguration?.SignalSearchTaxonListIds ??
                                        throw new ArgumentNullException(nameof(inputValaidationConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all Taxon list definitions.
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<TaxonListDefinitionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GetTaxonLists([FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var taxonLists = await _taxonListManager.GetTaxonListsAsync();

                if (!taxonLists?.Any() ?? true)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                var dtos = taxonLists.Select(m => TaxonListDefinitionDto.Create(m, cultureCode, _signalSearchTaxonListIds));
                return new OkObjectResult(dtos);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting projects");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get all taxa in a taxon list.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{taxonListId}/Taxa")]
        [ProducesResponseType(typeof(IEnumerable<TaxonListTaxonInformationDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [AzureApi, AzureInternalApi]
        public async Task<IActionResult> GetTaxa([FromRoute] int taxonListId)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var taxonLists = await _taxonListManager.GetTaxonListsAsync();

                if (!taxonLists?.Any() ?? true)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                var taxonList = taxonLists.FirstOrDefault(m => m.Id == taxonListId);
                if (taxonList == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return new OkObjectResult(taxonList.Taxa);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting taxa");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}