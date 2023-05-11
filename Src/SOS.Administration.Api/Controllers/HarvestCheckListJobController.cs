using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
    public class HarvestChecklistJobController : ControllerBase, IHarvestChecklistJobController
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
        public HarvestChecklistJobController(IDataProviderManager dataProviderManager,
            DwcaConfiguration dwcaConfiguration,
            ILogger<HarvestObservationsJobController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _dwcaConfiguration = dwcaConfiguration ?? throw new ArgumentNullException(nameof(dwcaConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
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

                var filename = FilenameHelper.CreateFilenameWithDate(model.DwcaFile.FileName, true);
                var filePath = Path.Combine(_dwcaConfiguration.ImportPath, filename);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);
                _logger.LogDebug("Start wait for DwC-A file transfer");
                Thread.Sleep(TimeSpan.FromSeconds(60)); // fix for slow network disk
                _logger.LogDebug("End wait for DwC-A file transfer");

                // process uploaded file
                BackgroundJob.Enqueue<IDwcArchiveHarvestJob>(job =>
                    job.RunAsync(dataProvider.Id, filePath, DwcaTarget.Checklist, JobCancellationToken.Null));
                return new OkObjectResult(
                    $"DwC-A harvest job for data provider: {dataProvider} was enqueued to Hangfire.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Enqueuing DwC-A harvest job failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}