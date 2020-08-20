using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Models;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Import;

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
        private readonly ILogger<ValidationController> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="dwcaConfiguration"></param>
        /// <param name="logger"></param>
        public ValidationController(
            DwcaConfiguration dwcaConfiguration,
            ILogger<ValidationController> logger)
        {
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                if (model.DwcaFile.Length == 0) return new BadRequestObjectResult("No file content");

                // Upload file and save temporarily on file system.
                string filename = FilenameHelper.CreateFilenameWithDate(model.DwcaFile.FileName, true);
                var filePath = Path.Combine(_dwcaConfiguration.ImportPath, filename);
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);

                // Enqueue job to Hangfire.
                BackgroundJob.Enqueue<ICreateDwcaDataValidationReportJob>(job =>
                    job.RunAsync(filePath, JobCancellationToken.Null));
                return new OkObjectResult($"Create DwC-A data validation report job for file \"{model.DwcaFile.FileName}\" was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DwC-A data validation report job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}