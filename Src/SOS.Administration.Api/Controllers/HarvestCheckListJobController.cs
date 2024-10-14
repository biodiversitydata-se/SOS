using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Controllers.Interfaces;
using SOS.Administration.Api.Models;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Jobs.Import;
using SOS.Lib.Repositories.Verbatim;
using System;
using System.IO;

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
        private readonly IVerbatimClient _verbatimClient;
        private readonly ILogger<HarvestObservationsJobController> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderManager"></param>
        /// <param name="verbatimClient"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public HarvestChecklistJobController(IDataProviderManager dataProviderManager,
            IVerbatimClient verbatimClient,
            ILogger<HarvestObservationsJobController> logger)
        {
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _verbatimClient = verbatimClient ?? throw new ArgumentNullException(nameof(verbatimClient));
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
                await using var stream = new MemoryStream();
                await model.DwcaFile.CopyToAsync(stream).ConfigureAwait(false);
                var darwinCoreArchiveVerbatimRepository = new DarwinCoreArchiveVerbatimRepository(dataProvider, _verbatimClient, _logger);
                if (!await darwinCoreArchiveVerbatimRepository.StoreSourceFileAsync(stream))
                {
                    return new BadRequestObjectResult("Failed to store sourec file");
                }

                // process uploaded file
                BackgroundJob.Enqueue<IDwcArchiveHarvestJob>(job =>
                    job.RunAsync(dataProvider.Id, DwcaTarget.Checklist, JobCancellationToken.Null));
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