using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Models;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Administration.Api.Controllers
{
    /// <summary>
    ///     Validation controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ValidationController : ControllerBase, IValidationController
    {
        private readonly DwcaConfiguration _dwcaConfiguration;
        private readonly IDataProviderManager _dataProviderManager;
        private readonly ILogger<ValidationController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public ValidationController(
            DwcaConfiguration dwcaConfiguration,
            IDataProviderManager dataProviderManager,
            ILogger<ValidationController> logger)
        {
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
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
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RunDataValidationJob(
            [FromQuery] string dataProviderIdOrIdentifier,
            [FromQuery] string createdBy,
            [FromQuery] int maxNrObservationsToRead = 100000,
            [FromQuery] int nrValidObservationsInReport = 10,
            [FromQuery] int nrInvalidObservationsInReport = 100)
        {
            try
            {
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
                _logger.LogError(e, $"{MethodBase.GetCurrentMethod()?.Name}() failed");
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
                if (model.DwcaFile == null) return new BadRequestObjectResult("No file is provided");
                if (model.DwcaFile.Length == 0) return new BadRequestObjectResult("No file content");
                if (model.MaxNrObservationsToRead <= 0) return new BadRequestObjectResult("MxNrObservationsToRead must be > 0");
                if (model.MaxNrObservationsToRead < model.NrInvalidObservationsInReport + model.NrInvalidObservationsInReport)
                    return new BadRequestObjectResult("MxNrObservationsToRead must be > NrInvalidObservationsInReport + NrInvalidObservationsInReport");

                // Upload file and save temporarily on file system.
                string filename = FilenameHelper.CreateFilenameWithDate(model.DwcaFile.FileName, true);
                var filePath = Path.Combine(_dwcaConfiguration.ImportPath, filename);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);
                var reportId = Report.CreateReportId();

                // Enqueue job to Hangfire.
                BackgroundJob.Enqueue<ICreateDwcaDataValidationReportJob>(job =>
                    job.RunAsync(
                        reportId, 
                        model.CreatedBy, 
                        filePath,
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
    }
}