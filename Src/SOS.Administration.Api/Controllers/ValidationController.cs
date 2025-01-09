﻿using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Models;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Verbatim;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Validation controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ValidationController : ControllerBase, IValidationController
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IVerbatimClient _verbatimClient;
        private readonly ILogger<ValidationController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="verbatimClient"></param>
        /// <param name="logger"></param>
        public ValidationController(
            IDataProviderManager dataProviderManager,
            IVerbatimClient verbatimClient,
            ILogger<ValidationController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create data validation report for a specific data provider.
        /// Prerequisite: The observations must have been harvested to MongoDB.
        /// </summary>
        /// <param name="dataProviderIdOrIdentifier">The Id or Identifier of the data provider.</param>
        /// <param name="createdBy">Name of the person that requested the report.</param>
        /// <param name="maxNrObservationsToRead">Max number of observations to read and process.</param>
        /// <param name="nrValidObservationsInReport">Max number of valid observations to include in report.</param>
        /// <param name="nrInvalidObservationsInReport">Max number of invalid observations to include in report.</param>
        /// <returns></returns>
        [HttpPost("CreateDataValidationReport/Run")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RunDataValidationJob(
            [FromQuery] string dataProviderIdOrIdentifier,
            [FromQuery] string createdBy,
            [FromQuery] int maxNrObservationsToRead = 100000,
            [FromQuery] int nrValidObservationsInReport = 10,
            [FromQuery] int nrInvalidObservationsInReport = 100)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var dataProvider =
                    await _dataProviderManager.GetDataProviderByIdOrIdentifier(dataProviderIdOrIdentifier);
                if (dataProvider == null)
                {
                    return new BadRequestObjectResult(
                        $"No data provider exist with Id={dataProviderIdOrIdentifier}");
                }

                if (maxNrObservationsToRead <= 0) return new BadRequestObjectResult("MxNrObservationsToRead must be > 0");
                if (maxNrObservationsToRead < nrInvalidObservationsInReport + nrInvalidObservationsInReport)
                    return new BadRequestObjectResult("MxNrObservationsToRead must be > NrInvalidObservationsInReport + NrInvalidObservationsInReport");
                var reportId = Report.CreateReportId();

                // Enqueue job to Hangfire.
                BackgroundJob.Enqueue<IDataValidationReportJob>(job =>
                    job.RunAsync(
                        reportId,
                        createdBy,
                        dataProvider.Identifier,
                        maxNrObservationsToRead,
                        nrValidObservationsInReport,
                        nrInvalidObservationsInReport,
                        JobCancellationToken.Null));

                return new OkObjectResult($"Create data validation report job for data provider \"{dataProvider}\" with Id \"{reportId}\" was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{@methodName}() failed", MethodBase.GetCurrentMethod()?.Name);                
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create data validation report for DwC-A files.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CreateDwcaDataValidationReport/Run")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RunDwcaDataValidationJob([FromForm] CreateDwcaDataValidationReportDto model)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                if (model.DwcaFile == null) return new BadRequestObjectResult("No file is provided");
                if (model.DwcaFile.Length == 0) return new BadRequestObjectResult("No file content");
                if (model.MaxNrObservationsToRead <= 0) return new BadRequestObjectResult("MxNrObservationsToRead must be > 0");
                if (model.MaxNrObservationsToRead < model.NrInvalidObservationsInReport + model.NrInvalidObservationsInReport)
                    return new BadRequestObjectResult("MxNrObservationsToRead must be > NrInvalidObservationsInReport + NrInvalidObservationsInReport");

                await using var stream = new MemoryStream();
                await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);

                var reportId = Report.CreateReportId();
                var darwinCoreArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(reportId, _verbatimClient, _logger);
                if (!await darwinCoreArchiveVerbatimRepository.StoreReportSourceFileAsync(stream))
                {
                    return new BadRequestObjectResult("Failed to store source file");
                }

                // Enqueue job to Hangfire.
                BackgroundJob.Enqueue<ICreateDwcaDataValidationReportJob>(job =>
                    job.RunAsync(
                        reportId,
                        model.CreatedBy,
                        model.MaxNrObservationsToRead,
                        model.NrValidObservationsInReport,
                        model.NrInvalidObservationsInReport,
                        model.NrTaxaInTaxonStatistics,
                        JobCancellationToken.Null));

                return new OkObjectResult($"Create DwC-A data validation report job for file \"{model.DwcaFile.FileName}\" with Id \"{reportId}\" was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DwC-A data validation report job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create Excel file with invalid observations.
        /// </summary>
        /// <returns></returns>
        [HttpPost("CreateExcelFileReport/Run")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RunCreateInvalidObservationsExcelFileJob([FromQuery] string dataProviderIdOrIdentifier, [FromQuery] string createdBy)
        {
            try
            {
                LogHelper.AddHttpContextItems(HttpContext, ControllerContext);
                var dataProvider =
                    await _dataProviderManager.GetDataProviderByIdOrIdentifier(dataProviderIdOrIdentifier);
                if (dataProvider == null)
                {
                    return new BadRequestObjectResult(
                        $"No data provider exist with Id={dataProviderIdOrIdentifier}");
                }

                var reportId = Report.CreateReportId();


                BackgroundJob.Enqueue<IInvalidObservationsReportsJob>(job => job.RunCreateExcelFileReportAsync(reportId, dataProvider.Id, createdBy));
                return new OkObjectResult($"Create Invalid Observations Excel report job with Id \"{reportId}\" was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing Invalid Observations Excel report job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

    }
}