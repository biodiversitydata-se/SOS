using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Shared.Api.Configuration;
using SOS.Shared.Api.Dtos;
using SOS.Shared.Api.Dtos.Status;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Controllers;

/// <summary>
///     Taxon lists controller
/// </summary>
[Route("[controller]")]
[ApiController]
public class TaxonListsController : ControllerBase
{
    private readonly ITaxonListManager _taxonListManager;
    private readonly ITaxonManager _taxonManager;
    private readonly IEnumerable<int> _signalSearchTaxonListIds;
    private readonly ILogger<TaxonListsController> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="taxonListManager"></param>
    /// <param name="taxonManager"></param>
    /// <param name="inputValaidationConfiguration"></param>
    /// <param name="logger"></param>
    public TaxonListsController(
        ITaxonListManager taxonListManager,
        ITaxonManager taxonManager,
        InputValaidationConfiguration inputValaidationConfiguration,
        ILogger<TaxonListsController> logger)
    {
        _taxonListManager = taxonListManager ?? throw new ArgumentNullException(nameof(taxonListManager));
        _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
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

    [HttpGet("{taxonListId}/sensitivity-statistics")]
    [InternalApi]
    public async Task<IActionResult> GetTaxonListSensitivityStatistics([FromRoute] int taxonListId)
    {
        try
        {
            var taxonLists = await _taxonListManager.GetTaxonListsAsync();
            var taxonList = taxonLists?.FirstOrDefault(m => m.Id == taxonListId);

            if (taxonList == null)
            {
                return NotFound($"Taxon list with id {taxonListId} was not found.");
            }

            var taxaByCategory = (taxonList.Taxa ?? Enumerable.Empty<TaxonListTaxonInformation>())
                .Where(t => t.SensitivityCategory.HasValue && t.SensitivityCategory.Value >= 3 && t.SensitivityCategory.Value <= 5)
                .GroupBy(t => t.SensitivityCategory!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => new TaxonListSensitivityStatisticsDto.TaxonInfo
                    {
                        Id = t.Id,
                        ScientificName = t.ScientificName,
                        SwedishName = t.SwedishName
                    }).OrderBy(t => t.ScientificName).ToList());

            var statistics = new TaxonListSensitivityStatisticsDto
            {
                Category3Taxa = taxaByCategory.GetValueOrDefault(3, []),
                Category4Taxa = taxaByCategory.GetValueOrDefault(4, []),
                Category5Taxa = taxaByCategory.GetValueOrDefault(5, []),
                Category3Count = taxaByCategory.GetValueOrDefault(3, []).Count,
                Category4Count = taxaByCategory.GetValueOrDefault(4, []).Count,
                Category5Count = taxaByCategory.GetValueOrDefault(5, []).Count
            };

            return Ok(statistics);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpGet("taxa/sensitivity-statistics")]
    [InternalApi]
    public async Task<IActionResult> GetAllTaxaSensitivityStatistics()
    {
        try
        {
            var taxonTree = await _taxonManager.GetTaxonTreeAsync();

            var taxaByCategory = taxonTree.TreeNodeById.Values
                .Where(n => n.Data?.Attributes?.SensitivityCategory != null)
                .Select(n => new { Node = n, Category = n.Data.Attributes.SensitivityCategory.Id })
                .Where(x => x.Category >= 3 && x.Category <= 5)
                .GroupBy(x => x.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new TaxonListSensitivityStatisticsDto.TaxonInfo
                    {
                        Id = x.Node.TaxonId,
                        ScientificName = x.Node.ScientificName,
                        SwedishName = x.Node.Data.VernacularName
                    }).OrderBy(t => t.ScientificName).ToList());

            var statistics = new TaxonListSensitivityStatisticsDto
            {
                Category3Taxa = taxaByCategory.GetValueOrDefault(3, []),
                Category4Taxa = taxaByCategory.GetValueOrDefault(4, []),
                Category5Taxa = taxaByCategory.GetValueOrDefault(5, []),
                Category3Count = taxaByCategory.GetValueOrDefault(3, []).Count,
                Category4Count = taxaByCategory.GetValueOrDefault(4, []).Count,
                Category5Count = taxaByCategory.GetValueOrDefault(5, []).Count
            };

            return Ok(statistics);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}