using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Serializers;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Vocabulary;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

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
        private readonly ILogger<TaxonListsController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="taxonListManager"></param>
        /// <param name="logger"></param>
        public TaxonListsController(
            ITaxonListManager taxonListManager,
            ILogger<TaxonListsController> logger)
        {
            _taxonListManager = taxonListManager ?? throw new ArgumentNullException(nameof(taxonListManager));
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
        public async Task<IActionResult> GetTaxonLists([FromQuery] string cultureCode = "en-GB")
        {
            try
            {
                var taxonLists = await _taxonListManager.GetTaxonListsAsync();

                if (!taxonLists?.Any() ?? true)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                var dtos = taxonLists.Select(m => TaxonListDefinitionDto.Create(m, cultureCode));
                return new OkObjectResult(dtos);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting projects");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get all Taxon list definitions.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{taxonListId}/Taxa")]
        [ProducesResponseType(typeof(IEnumerable<TaxonListTaxonInformationDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetTaxa([FromRoute] int taxonListId)
        {
            try
            {
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