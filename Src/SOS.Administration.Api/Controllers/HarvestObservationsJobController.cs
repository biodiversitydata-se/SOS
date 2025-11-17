using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Models;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Verbatim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SOS.Administration.Api.Controllers;

/// <summary>
///     Harvest observations job controller
/// </summary>
[ApiController]
[Route("[controller]")]
public class HarvestObservationsJobController : ControllerBase, IHarvestObservationJobController
{
    private readonly IDataProviderManager _dataProviderManager;
    private readonly IVerbatimClient _verbatimClient;
    private readonly ILogger<HarvestObservationsJobController> _logger;

   /// <summary>
   /// Constructor
   /// </summary>
   /// <param name="dataProviderManager"></param>
   /// <param name="verbatimClient"></param>
   /// <param name="logger"></param>
   /// <exception cref="ArgumentNullException"></exception>
    public HarvestObservationsJobController(IDataProviderManager dataProviderManager,
        IVerbatimClient verbatimClient,
        ILogger<HarvestObservationsJobController> logger)
    {
        _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
        _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    [HttpPost("Run")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> RunObservationsHarvestJob([FromQuery] List<string> dataProviderIdOrIdentifiers)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            if (dataProviderIdOrIdentifiers == null || dataProviderIdOrIdentifiers.Count == 0)
            {
                return new BadRequestObjectResult("dataProviderIdOrIdentifiers is not set");
            }

            var parsedDataProvidersResult =
                await _dataProviderManager.GetDataProvidersByIdOrIdentifier(dataProviderIdOrIdentifiers);
            var parsedDataProvidersCombinedResult = Result.Combine(parsedDataProvidersResult);
            if (parsedDataProvidersCombinedResult.IsFailure)
            {
                return new BadRequestObjectResult(parsedDataProvidersCombinedResult.Error);
            }

            if (parsedDataProvidersResult.Any(providerResult =>
                providerResult.Value.Type == DataProviderType.DwcA &&
                string.IsNullOrEmpty(providerResult.Value.Datasets?.FirstOrDefault(ds => ds.Type.Equals(DataProviderDataset.DatasetType.Observations))?.DataUrl))
            )
            {
                return new BadRequestObjectResult(
                    "One or more DwC-A data provider/s with missing download url was included in the list. Harvesting of these providers is only supported by providing a file using the Administration API.");
            }

            BackgroundJob.Enqueue<IObservationsHarvestJobFull>(job => job.RunHarvestObservationsAsync(
                parsedDataProvidersResult.Select(providerResult => providerResult.Value.Identifier).ToList(),
                JobCancellationToken.Null));

            return new OkObjectResult(
                $"Harvest observations job enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, parsedDataProvidersResult.Select(res => " - " + res.Value))}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Enqueuing process job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    /// <inheritdoc />
    [HttpPost("Schedule/Daily")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> AddObservationsHarvestJob([FromQuery] List<string> dataProviderIdOrIdentifiers,
        [FromQuery] int hour, [FromQuery] int minute)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            if (dataProviderIdOrIdentifiers == null || dataProviderIdOrIdentifiers.Count == 0)
            {
                return new BadRequestObjectResult("dataProviderIdOrIdentifiers is not set");
            }

            var parsedDataProvidersResult =
                await _dataProviderManager.GetDataProvidersByIdOrIdentifier(dataProviderIdOrIdentifiers);
            var parsedDataProvidersCombinedResult = Result.Combine(parsedDataProvidersResult);
            if (parsedDataProvidersCombinedResult.IsFailure)
            {
                return new BadRequestObjectResult(parsedDataProvidersCombinedResult.Error);
            }

            if (parsedDataProvidersResult.Any(providerResult => providerResult.Value.Type == DataProviderType.DwcA))
            {
                return new BadRequestObjectResult(
                    "A DwC-A data provider was included in the list. Currently DwC-A harvesting is only supported by providing a file using the Administration API.");
            }

            RecurringJob.AddOrUpdate<IObservationsHarvestJobFull>(
                nameof(IObservationsHarvestJobFull),
                job => job.RunHarvestObservationsAsync(
                    parsedDataProvidersResult.Select(providerResult => providerResult.Value.Identifier).ToList(),
                    JobCancellationToken.Null),
                $"0 {minute} {hour} * * ?", new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            return new OkObjectResult(
                $"Scheduled harvest observations job enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, parsedDataProvidersResult.Select(res => " - " + res.Value))}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Enqueuing process job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    #region DwC-A

    /// <summary>
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("DwcArchive/Run")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [DisableRequestSizeLimit]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> RunDwcArchiveHarvestJob([FromForm] UploadDwcArchiveModelDto model)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            var dataProvider =
                await _dataProviderManager.GetDataProviderByIdOrIdentifier(model.DataProviderIdOrIdentifier);
            if (dataProvider == null)
            {
                return new BadRequestObjectResult(
                    $"No data provider exist with Id={model.DataProviderIdOrIdentifier}");
            }

            if (dataProvider.Type != DataProviderType.DwcA)
            {
                return new BadRequestObjectResult($"The data provider \"{dataProvider}\" is not a DwC-A provider");
            }

            if (model.DwcaFile.Length == 0)
            {
                return new BadRequestObjectResult("No file content");
            }

            await using var stream = new MemoryStream();
            await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);
            var darwinCoreArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(dataProvider, _verbatimClient, _logger);
            if (!await darwinCoreArchiveVerbatimRepository.StoreSourceFileAsync(stream))
            {
                return new BadRequestObjectResult("Failed to store sourec file");
            }

            // process uploaded file                
            BackgroundJob.Enqueue<IDwcArchiveHarvestJob>(job =>
                job.RunAsync(dataProvider.Id, DwcaTarget.Observation, JobCancellationToken.Null));
            return new OkObjectResult(
                $"DwC-A harvest job for data provider: {dataProvider} was enqueued to Hangfire.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Enqueuing DwC-A harvest job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    #endregion DwC-A

    [HttpPost("FulliNaturalist/Run")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public IActionResult RuniNaturalistFullObservationsHarvestJob()
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);                
            BackgroundJob.Enqueue<IObservationsHarvestJobFull>(job => job.RunFulliNaturalistHarvestObservationsAsync(                    
                JobCancellationToken.Null));
            return new OkObjectResult("Harvest full iNaturalist observations job enqueued to Hangfire");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Enqueuing harvest full iNaturalist observations job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost("FulliNaturalist/Schedule")]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public IActionResult ScheduleiNaturalistFullObservationsHarvestJob([FromQuery] int days, [FromQuery] int hour, [FromQuery] int minute)
    {
        try
        {
            LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
            string cronExpression = $"0 {minute} {hour} */{days} * ?";
            RecurringJob.AddOrUpdate<IObservationsHarvestJobFull>($"{nameof(IObservationsHarvestJobFull)}-iNaturalist",
                job => job.RunFulliNaturalistHarvestObservationsAsync(JobCancellationToken.Null), cronExpression,
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });

            return new OkObjectResult("Harvest full iNaturalist observations job scheduled to Hangfire");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Scheduled harvest full iNaturalist observations job failed");
            return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
        }
    }
}