using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Models;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Harvest observations job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HarvestObservationsJobController : ControllerBase, IHarvestObservationJobController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly ILogger<HarvestObservationsJobController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="logger"></param>
        public HarvestObservationsJobController(IDataProviderManager dataProviderManager,
            DwcaConfiguration dwcaConfiguration,
            ILogger<HarvestObservationsJobController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpPost("Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunObservationsHarvestJob([FromQuery] List<string> dataProviderIdOrIdentifiers)
        {
            try
            {
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

                BackgroundJob.Enqueue<IObservationsHarvestJob>(job => job.RunHarvestObservationsAsync(
                    parsedDataProvidersResult.Select(providerResult => providerResult.Value.Identifier).ToList(),
                    JobCancellationToken.Null));

                return new OkObjectResult(
                    $"Harvest observations job enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, parsedDataProvidersResult.Select(res => " - " + res.Value))}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Schedule/Daily")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AddObservationsHarvestJob([FromQuery] List<string> dataProviderIdOrIdentifiers,
            [FromQuery] int hour, [FromQuery] int minute)
        {
            try
            {
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

                RecurringJob.AddOrUpdate<IObservationsHarvestJob>(
                    nameof(IObservationsHarvestJob),
                    job => job.RunHarvestObservationsAsync(
                        parsedDataProvidersResult.Select(providerResult => providerResult.Value.Identifier).ToList(),
                        JobCancellationToken.Null),
                    $"0 {minute} {hour} * * ?", TimeZoneInfo.Local);

                return new OkObjectResult(
                    $"Scheduled harvest observations job enqueued to Hangfire with the following data providers:{Environment.NewLine}{string.Join(Environment.NewLine, parsedDataProvidersResult.Select(res => " - " + res.Value))}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing process job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #region DwC-A

        /// <summary>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("DwcArchive/Run")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [DisableRequestSizeLimit]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RunDwcArchiveHarvestJob([FromForm] UploadDwcArchiveModelDto model)
        {
            try
            {
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

                string filename = FilenameHelper.CreateFilenameWithDate(model.DwcaFile.FileName, true);
                var filePath = Path.Combine(_dwcaConfiguration.ImportPath, filename);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);

                // process uploaded file
                BackgroundJob.Enqueue<IDwcArchiveHarvestJob>(job =>
                    job.RunAsync(dataProvider.Id, filePath, JobCancellationToken.Null));
                return new OkObjectResult(
                    $"DwC-A harvest job for data provider: {dataProvider} was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing DwC-A harvest job failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        #endregion DwC-A
    }
}