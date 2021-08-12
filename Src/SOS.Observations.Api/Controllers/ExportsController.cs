using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Controllers.Interfaces;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    ///     Import job controller
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ExportsController : ObservationBaseController, IExportsController
    {
        private readonly IBlobStorageManager _blobStorageManager;
        private readonly IExportManager _exportManager;
        private readonly IFileService _fileService;
        private readonly long _orderExportObservationsLimit;
        private readonly long _downloadExportObservationsLimit;
        private readonly string _exportPath;
        private readonly ILogger<ExportsController> _logger;

        /// <summary>
        /// Populate output fields based on property set
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="exportPropertySet"></param>
        private void PopulateOutputFields(SearchFilter filter, ExportPropertySet exportPropertySet)
        {
            if (exportPropertySet == ExportPropertySet.All)
            {
                return;
            }

            var outputFields = new List<string>
            {
                "datasetName",
                "event.startDate",
                "event.endDate",
                "identification.validated",
                "location.decimalLongitude",
                "location.decimalLatitude",
                "occurrence.occurrenceId",
                "occurrence.reportedBy",
                "taxon.id",
                "taxon.scientificName",
                "taxon.vernacularName"
            };

            if (exportPropertySet == ExportPropertySet.Extended)
            {
                outputFields.Add("location.county");
                outputFields.Add("location.locality");
                outputFields.Add("location.municipality");
                outputFields.Add("location.province");
                outputFields.Add("location.parish");
                outputFields.Add("taxon.kingdom");
                outputFields.Add("taxon.organismGroup");
                outputFields.Add("taxon.redlistCategory");
            }

            filter.OutputFields = outputFields;
        }

        /// <summary>
        /// Validate input for order request
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private async Task<IActionResult> OrderValidateAsync(ExportFilterDto filter)
        {
            var email = User?.Claims?.FirstOrDefault(c => c.Type.Contains("emailaddress", StringComparison.CurrentCultureIgnoreCase))?.Value;

            var validationResults = Result.Combine(
                ValidateSearchFilter(filter),
                ValidateEmail(email));

            if (validationResults.IsFailure)
            {
                return BadRequest(validationResults.Error);
            }
            
            var exportFilter = filter.ToSearchFilter("en-GB", false);
            var matchCount = await ObservationManager.GetMatchCountAsync(null, exportFilter);

            if (matchCount == 0)
            {
                return NoContent();
            }

            if (matchCount > _orderExportObservationsLimit)
            {
                return BadRequest($"Query exceeds limit of {_orderExportObservationsLimit} observations.");
            }
            
            return new OkObjectResult((email, exportFilter));
        }

        /// <summary>
        /// Validate input for download request
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private async Task<IActionResult> DownloadValidateAsync(ExportFilterDto filter)
        {
            
            if (ValidateSearchFilter(filter).IsFailure)
            {
                return BadRequest(ValidateSearchFilter(filter).Error);
            }

            var exportFilter = filter.ToSearchFilter("en-GB", false);
            var matchCount = await ObservationManager.GetMatchCountAsync(null, exportFilter);

            if (matchCount == 0)
            {
                return NoContent();
            }

            if (matchCount > _downloadExportObservationsLimit)
            {
                return BadRequest($"Query exceeds limit of {_downloadExportObservationsLimit} observations.");
            }

            return new OkObjectResult(exportFilter);
        }

        /// <summary>
        /// Return file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private IActionResult GetFile(string filePath, string fileName)
        {
            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, "application/zip", fileName);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="blobStorageManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="exportManager"></param>
        /// <param name="fileService"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public ExportsController(IObservationManager observationManager,
            IBlobStorageManager blobStorageManager,
            IAreaManager areaManager,
            ITaxonManager taxonManager,
            IExportManager exportManager,
            IFileService fileService,
            ObservationApiConfiguration configuration,
            ILogger<ExportsController> logger) : base(observationManager, areaManager, taxonManager)
        {
            _blobStorageManager = blobStorageManager ?? throw new ArgumentNullException(nameof(blobStorageManager));
            _exportManager = exportManager ?? throw new ArgumentNullException(nameof(exportManager));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _orderExportObservationsLimit = configuration?.OrderExportObservationsLimit ?? throw new ArgumentNullException(nameof(configuration));
            _downloadExportObservationsLimit = configuration.DownloadExportObservationsLimit;
            _exportPath = configuration.ExportPath;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [HttpGet("Datasets")]
        [ProducesResponseType(typeof(IEnumerable<Lib.Models.Misc.File>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDatasetsList()
        {
            try
            {
                var files = await _blobStorageManager.GetExportFilesAsync();

                if (!files?.Any() ?? true)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return new OkObjectResult(files);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting export files");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Download/DwC")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DownloadDwCAsync([FromBody] ExportFilterDto filter)
        {
            var filePath = string.Empty;
            try
            {
                var validateResult = await DownloadValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;

                filePath =
                    await _exportManager.CreateExportFileAsync(exportFilter, ExportFormat.DwC, _exportPath,
                        JobCancellationToken.Null);
                return GetFile(filePath, "Observations_DwC.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error exporting DwC file");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
            finally
            {
                _fileService.DeleteFile(filePath);
            }
        }

        /// <inheritdoc />
        [HttpPost("Download/Excel")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DownloadExcelAsync([FromBody] ExportFilterDto filter, [FromQuery] ExportPropertySet exportPropertySet)
        {
            var filePath = string.Empty;
            try
            {
                var validateResult = await DownloadValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                PopulateOutputFields(exportFilter, exportPropertySet);

                filePath =
                    await _exportManager.CreateExportFileAsync(exportFilter, ExportFormat.Excel, _exportPath,
                        JobCancellationToken.Null);
                return GetFile(filePath, "Observations_Excel.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error exporting Excel file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                _fileService.DeleteFile(filePath);
            }
        }

        /// <inheritdoc />
        [HttpPost("Download/GeoJson")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DownloadGeoJsonAsync([FromBody] ExportFilterDto filter, [FromQuery] ExportPropertySet exportPropertySet)
        {
            var filePath = string.Empty;
            try
            {
                var validateResult = await DownloadValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                PopulateOutputFields(exportFilter, exportPropertySet);

                filePath =
                    await _exportManager.CreateExportFileAsync(exportFilter, ExportFormat.GeoJson, _exportPath,
                        JobCancellationToken.Null);
                return GetFile(filePath, "Observations_GeoJson.zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error exporting GeoJson file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                _fileService.DeleteFile(filePath);
            }
        }

        /// <inheritdoc />
        [HttpPost("Order/DwC")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> OrderDwCAsync([FromBody] ExportFilterDto filter, [FromQuery] string description)
        {
            try
            {
                var validateResult = await OrderValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var (email, exportFilter) = ((string, SearchFilter))okResult.Value;

                return new OkObjectResult(BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, email, description, ExportFormat.DwC, JobCancellationToken.Null)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Order/Excel")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> OrderExcelAsync([FromBody] ExportFilterDto filter, [FromQuery] string description, [FromQuery] ExportPropertySet exportPropertySet)
        {
            try
            {
                var validateResult = await OrderValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var (email, exportFilter) = ((string, SearchFilter))okResult.Value;
                PopulateOutputFields(exportFilter, exportPropertySet);

                return new OkObjectResult(BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, email, description, ExportFormat.Excel, JobCancellationToken.Null)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Order/GeoJson")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> OrderGeoJsonAsync([FromBody] ExportFilterDto filter, [FromQuery] string description, [FromQuery] ExportPropertySet exportPropertySet)
        {
            try
            {
                var validateResult = await OrderValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var (email, exportFilter) = ((string, SearchFilter))okResult.Value;
                PopulateOutputFields(exportFilter, exportPropertySet);

                return new OkObjectResult(BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, email, description, ExportFormat.GeoJson, JobCancellationToken.Null)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}