using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Shared;
using SOS.Lib.Managers.Interfaces;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SOS.Administration.Api.Controllers;

/// <summary>
///     Import job controller
/// </summary>
[ApiController]
[Route("[controller]")]
public class CachesController : ControllerBase, ICachesController
{
    private readonly ICacheManager _cacheManager;
    private readonly ILogger<CachesController> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cacheManager"></param>
    /// <param name="logger"></param>
    public CachesController(ICacheManager cacheManager, ILogger<CachesController> logger)
    {
        _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    [HttpDelete("{cache}")]
    // [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> ClearAsync([FromRoute] Cache cache)
    {
        LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
        return new OkObjectResult("THIS ENDPOINT IS TEMPORARILY DISABLED");
        // try
        // {
        //     BackgroundJob.Enqueue<IClearCacheJob>(job => job.RunAsync(new[] { cache }));
        //     return new OkObjectResult(await _cacheManager.ClearAsync(cache));
        // }
        // catch (Exception e)
        // {
        //     _logger.LogError(e, $"Failed to clear {cache} cache");
        //     return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        // }
    }
}