using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;

namespace SOS.Administration.Api.Controllers;

/// <summary>
///     Import job controller
/// </summary>
[ApiController]
[Route("[controller]")]
public class HarvestAndProcessJobController : ControllerBase, IHarvestAndProcessJobController
{
    private readonly IDataProviderManager _dataProviderManager;
    private readonly ILogger<HarvestAndProcessJobController> _logger;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="dataProviderManager"></param>
    /// <param name="logger"></param>
    public HarvestAndProcessJobController(
        IDataProviderManager dataProviderManager,
        ILogger<HarvestAndProcessJobController> logger)
    {
        _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    [HttpPost("Observations/Schedule/Daily")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public IActionResult AddDailyObservationHarvestAndProcessJob([FromQuery] int hour, [FromQuery] int minute)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            RecurringJob.AddOrUpdate<IObservationsHarvestJobFull>($"{nameof(IObservationsHarvestJobFull)}",
                job => job.RunFullAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            return new OkObjectResult("Observations harvest and process job added");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Adding observations harvest and process job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <inheritdoc />
    [HttpPost("Observations/Run")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public IActionResult RunObservationHarvestAndProcessJob()
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            BackgroundJob.Enqueue<IObservationsHarvestJobFull>(job => job.RunFullAsync(JobCancellationToken.Null));
            return new OkObjectResult("Observations harvest and process job was enqueued to Hangfire.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Enqueuing observations harvest and process job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <inheritdoc />
    [HttpPost("Observations/Run/Incremental")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public IActionResult RunIncrementalObservationHarvestAndProcessJob(DateTime? fromDate)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            BackgroundJob.Enqueue<IObservationsHarvestJobIncremental>(job => job.RunIncrementalActiveAsync(fromDate,
              JobCancellationToken.Null));

            return new OkObjectResult("Incremental observation Harvest and process job enqued");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Enquing incremental observation Harvest and process job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost("Observations/Run/Incremental/Artportalen")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public IActionResult RunIncrementalArtportalenObservationHarvestAndProcessJob([FromBody] List<int> ids)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            BackgroundJob.Enqueue<IObservationsHarvestJobIncremental>(job => job.RunHarvestArtportalenObservationsAsync(ids,
                JobCancellationToken.Null));

            return new OkObjectResult("Incremental observation Harvest and process job for specified Artportalen id's enqued");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Incremental observation Harvest and process job for specified Artportalen id's failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <inheritdoc />
    [HttpPost("Observations/Schedule/Incremental")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public IActionResult ScheduleIncrementalObservationHarvestAndProcessJob([FromQuery] byte runIntervalInMinutes)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            if (runIntervalInMinutes <= 0 || runIntervalInMinutes > 59)
            {

                return new BadRequestObjectResult("Run interval must be between 1 and 59");
            }

            RecurringJob.AddOrUpdate<IObservationsHarvestJobIncremental>(
                $"{nameof(IObservationsHarvestJobIncremental)}", job => job.RunIncrementalActiveAsync(null, JobCancellationToken.Null),
                $"*/{runIntervalInMinutes} * * * *", new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            return new OkObjectResult("Incremental observation Harvest and process job scheduled");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Scheduling incremental observation Harvest and process job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <inheritdoc />
    [HttpPost("Checklists/Schedule/Daily")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public IActionResult AddDailyChecklistHarvestAndProcessJob([FromQuery] int hour, [FromQuery] int minute)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            RecurringJob.AddOrUpdate<IChecklistsHarvestJob>($"{nameof(IChecklistsHarvestJob)}-Full",
                job => job.RunAsync(JobCancellationToken.Null), $"0 {minute} {hour} * * ?", new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
            return new OkObjectResult("Check lists harvest and process job added");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Adding checklists harvest and process job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <inheritdoc />
    [HttpPost("Checklists/Run")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public IActionResult RunChecklistHarvestAndProcessJob()
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            BackgroundJob.Enqueue<IChecklistsHarvestJob>(job => job.RunAsync(JobCancellationToken.Null));
            return new OkObjectResult("Check lists harvest and process job was enqueued to Hangfire.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Enqueuing checklists harvest and process job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}