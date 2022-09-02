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
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
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
        private readonly IUserExportRepository _userExportRepository;
        private readonly int _defaultUserExportLimit;
        private readonly long _orderExportObservationsLimit;
        private readonly long _downloadExportObservationsLimit;
        private readonly string _exportPath;
        private readonly ILogger<ExportsController> _logger;

        private string UserEmail => User?.Claims?.FirstOrDefault(c => c.Type.Contains("emailaddress", StringComparison.CurrentCultureIgnoreCase))?.Value;

        private int UserId => int.Parse(User?.Claims?.FirstOrDefault(c => c.Type.Contains("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.CurrentCultureIgnoreCase))?.Value ?? "0");

        /// <summary>
        /// Use outputFieldSet passed in query if no value is passed in filter
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="outputFieldSet"></param>
        private void HandleOutputFieldSet(SearchFilterDto filter,
              OutputFieldSet outputFieldSet)
        {
            if (outputFieldSet != OutputFieldSet.None)
            {
                filter = filter ?? new SearchFilterDto();
                filter.Output = filter?.Output ?? new OutputFilterDto();

                if ((filter.Output.FieldSet ?? OutputFieldSet.None) == OutputFieldSet.None)
                {
                    filter.Output.FieldSet = outputFieldSet;
                }
            }
        }

        /// <summary>
        /// Get user export info
        /// </summary>
        /// <returns></returns>
        private async Task<UserExport> GetUserExportsAsync()
        {
            var userExport = await _userExportRepository.GetAsync(UserId);
            return userExport ?? new UserExport { Id = UserId, Limit = _defaultUserExportLimit };
        }

        /// <summary>
        /// Update user exports
        /// </summary>
        /// <param name="userExport"></param>
        /// <returns></returns>
        private async Task UpdateUserExportsAsync(UserExport userExport)
        {
            await _userExportRepository.AddOrUpdateAsync(userExport);
        }

        /// <summary>
        /// Validate input for order request
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="email"></param>
        /// <param name="userExport"></param>
        /// <returns></returns>
        private async Task<(IActionResult Result, long? Count)> OrderValidateAsync(SearchFilterDto filter, string email, UserExport userExport)
        {
            var validationResults = Result.Combine(
                ValidateSearchFilter(filter),
                ValidateEmail(email),
                ValidateUserExport(userExport));

            if (validationResults.IsFailure)
            {
                return (Result: BadRequest(validationResults.Error), Count: null);
            }

            var exportFilter = filter.ToSearchFilter("en-GB", false);
            var matchCount = await ObservationManager.GetMatchCountAsync(0, null, exportFilter);

            if (matchCount == 0)
            {
                return (Result: NoContent(), Count: 0);
            }

            if (matchCount > _orderExportObservationsLimit)
            {
                return (Result: BadRequest($"Query exceeds limit of {_orderExportObservationsLimit} observations."), Count: matchCount);
            }

            return (Result: new OkObjectResult(exportFilter), Count: matchCount);            
        }

        /// <summary>
        /// Validate input for download request
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        private async Task<IActionResult> DownloadValidateAsync(SearchFilterDto filter)
        {
            var validationResults = ValidateSearchFilter(filter);

            if (validationResults.IsFailure)
            {
                return BadRequest(ValidateSearchFilter(filter).Error);
            }

            var exportFilter = filter.ToSearchFilter("en-GB", false);
            var matchCount = await ObservationManager.GetMatchCountAsync(0, null, exportFilter);

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
        /// Validate user export
        /// </summary>
        /// <param name="userExport"></param>
        /// <returns></returns>
        private Result ValidateUserExport(UserExport userExport)
        {
            if ((userExport?.OnGoingJobIds?.Count() ?? 0) > (userExport?.Limit ?? 1))
            {
                return Result.Failure($"User already has {userExport.OnGoingJobIds.Count()} on going exports.");
            }

            return Result.Success();
        }

        /// <summary>
        /// Return file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private IActionResult GetFile(string filePath, string fileName, string contentType)
        {
            var bytes = System.IO.File.ReadAllBytes(filePath);
            return File(bytes, contentType, fileName);
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
        /// <param name="userExportRepository"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public ExportsController(IObservationManager observationManager,
            IBlobStorageManager blobStorageManager,
            IAreaManager areaManager,
            ITaxonManager taxonManager,
            IExportManager exportManager,
            IFileService fileService,
            IUserExportRepository userExportRepository,
            ObservationApiConfiguration configuration,
            ILogger<ExportsController> logger) : base(observationManager, areaManager, taxonManager)
        {
            _blobStorageManager = blobStorageManager ?? throw new ArgumentNullException(nameof(blobStorageManager));
            _exportManager = exportManager ?? throw new ArgumentNullException(nameof(exportManager));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _userExportRepository =
                userExportRepository ?? throw new ArgumentNullException(nameof(userExportRepository));
            _defaultUserExportLimit = configuration?.DefaultUserExportLimit ?? throw new ArgumentNullException(nameof(configuration));
            _orderExportObservationsLimit = configuration.OrderExportObservationsLimit;
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
        [HttpPost("Download/Csv")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> DownloadCsv(
             [FromBody] SearchFilterDto filter,
             [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
             [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
             [FromQuery] string cultureCode = "sv-SE",
             [FromQuery] bool gzip = true)
        {
            HandleOutputFieldSet(filter, outputFieldSet);
            cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
            FileExportResult fileExportResult = null;
            try
            {
                var validateResult = await DownloadValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
               
                fileExportResult =
                    await _exportManager.CreateExportFileAsync(exportFilter, ExportFormat.Csv,
                        _exportPath, cultureCode,
                        false,
                        propertyLabelType,
                        false,
                        gzip,
                        JobCancellationToken.Null);

                HttpContext.LogObservationCount(fileExportResult.NrObservations);
                if (gzip)
                    return GetFile(fileExportResult.FilePath, "Observations_Csv.zip", "application/zip");
                else
                    return GetFile(fileExportResult.FilePath, "Observations.csv", "text/tab-separated-values"); // or "text/csv"?                
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error exporting Csv file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                if (fileExportResult != null) _fileService.DeleteFile(fileExportResult.FilePath);
            }
        }

        /// <inheritdoc />
        [HttpPost("Download/DwC")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> DownloadDwC([FromBody] SearchFilterDto filter)
        {
            FileExportResult fileExportResult = null;
            try
            {               
                var validateResult = await DownloadValidateAsync(filter);
                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                fileExportResult =
                    await _exportManager.CreateExportFileAsync(exportFilter, 
                        ExportFormat.DwC,
                        _exportPath, 
                        Cultures.en_GB, 
                        false,
                        PropertyLabelType.PropertyPath,
                        false,
                        true,
                        JobCancellationToken.Null);

                HttpContext.LogObservationCount(fileExportResult.NrObservations);
                return GetFile(fileExportResult.FilePath, "Observations_DwC.zip", "application/zip");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error exporting DwC file");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
            finally
            {
                if (fileExportResult != null) _fileService.DeleteFile(fileExportResult.FilePath);
            }
        }

        /// <inheritdoc />
        [HttpPost("Download/Excel")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> DownloadExcel(
            [FromBody] SearchFilterDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName, 
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool gzip = true)
        {
            HandleOutputFieldSet(filter, outputFieldSet);
            cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
            FileExportResult fileExportResult = null;
            try
            {
                var validateResult = await DownloadValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                fileExportResult =
                    await _exportManager.CreateExportFileAsync(exportFilter, ExportFormat.Excel, 
                        _exportPath, cultureCode,
                        false,
                        propertyLabelType,
                        false,
                        gzip,
                        JobCancellationToken.Null);

                HttpContext.LogObservationCount(fileExportResult.NrObservations);
                if (gzip)
                    return GetFile(fileExportResult.FilePath, "Observations_Excel.zip", "application/zip");
                else
                    return GetFile(fileExportResult.FilePath, "Observations.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error exporting Excel file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                if (fileExportResult != null) _fileService.DeleteFile(fileExportResult.FilePath);
            }
        }

        /// <inheritdoc />
        [HttpPost("Download/GeoJson")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> DownloadGeoJson([FromBody] SearchFilterDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool flat = true,
            [FromQuery] bool excludeNullValues = true,
            [FromQuery] bool gzip = true)
        {
            HandleOutputFieldSet(filter, outputFieldSet);
            cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
            FileExportResult fileExportResult = null;
            
            try
            {
                var validateResult = await DownloadValidateAsync(filter);
                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;

                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        exportFilter, 
                        ExportFormat.GeoJson, 
                        _exportPath, 
                        cultureCode, 
                        flat,
                        propertyLabelType,
                        excludeNullValues,
                        gzip,
                        JobCancellationToken.Null);

                HttpContext.LogObservationCount(fileExportResult?.NrObservations ?? 0);
                if (gzip)                
                    return GetFile(fileExportResult.FilePath, "Observations_GeoJson.zip", "application/zip");
                else                
                    return GetFile(fileExportResult.FilePath, "Observations.geojson", "application/geo+json");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error exporting GeoJson file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                if (fileExportResult != null) _fileService.DeleteFile(fileExportResult.FilePath);
            }
        }

        /// <inheritdoc />
        [HttpPost("Order/Csv")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> OrderCsv([FromBody] SearchFilterDto filter,
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, UserEmail, userExports);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, UserId, UserEmail, description, ExportFormat.Csv, cultureCode, false,
                        propertyLabelType, false, null, JobCancellationToken.Null));

                userExports.OnGoingJobIds.Add(jobId);
                var exportJobInfo = new ExportJobInfo
                {
                    Id = jobId,
                    Status = ExportJobStatus.Queued,
                    CreatedDate = DateTime.UtcNow,
                    NumberOfObservations = Convert.ToInt32(validateResult.Count),
                    Format = ExportFormat.Csv,
                    Description = description,
                    OutputFieldSet = filter?.Output?.FieldSet
                };

                if (userExports.Jobs == null) userExports.Jobs = new List<ExportJobInfo>();
                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        [HttpPost("Order/DwC")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> OrderDwC([FromBody] SearchFilterDto filter, [FromQuery] string description)
        {
            try
            {
                var userExports = await GetUserExportsAsync();
                var validateResult = await OrderValidateAsync(filter, UserEmail, userExports);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, UserId, UserEmail, description, ExportFormat.DwC, "en-GB", false,
                        PropertyLabelType.PropertyName, false, null, JobCancellationToken.Null));

                userExports.OnGoingJobIds.Add(jobId);
                var exportJobInfo = new ExportJobInfo
                {
                    Id = jobId,
                    Status = ExportJobStatus.Queued,
                    CreatedDate = DateTime.UtcNow,
                    NumberOfObservations = Convert.ToInt32(validateResult.Count),
                    Format = ExportFormat.DwC,
                    Description = description                    
                };

                if (userExports.Jobs == null) userExports.Jobs = new List<ExportJobInfo>();
                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
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
        public async Task<IActionResult> OrderExcel([FromBody] SearchFilterDto filter, 
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName, 
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, UserEmail, userExports);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var exportFilter = (SearchFilter)okResult.Value;

                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, UserId, UserEmail, description, ExportFormat.Excel, cultureCode, false,
                        propertyLabelType, false, null, JobCancellationToken.Null));

                userExports.OnGoingJobIds.Add(jobId);
                var exportJobInfo = new ExportJobInfo
                {
                    Id = jobId,
                    Status = ExportJobStatus.Queued,
                    CreatedDate = DateTime.UtcNow,
                    NumberOfObservations = Convert.ToInt32(validateResult.Count),
                    Format = ExportFormat.Excel,
                    Description = description,
                    OutputFieldSet = filter?.Output?.FieldSet                    
                };

                if (userExports.Jobs == null) userExports.Jobs = new List<ExportJobInfo>();
                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
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
        public async Task<IActionResult> OrderGeoJson([FromBody] SearchFilterDto filter, 
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool flat = true,
            bool excludeNullValues = true)
        {
            try
            {
                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, UserEmail, userExports);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, UserId, UserEmail, description, ExportFormat.GeoJson, cultureCode,
                        flat, propertyLabelType, excludeNullValues, null, JobCancellationToken.Null));

                userExports.OnGoingJobIds.Add(jobId);
                var exportJobInfo = new ExportJobInfo
                {
                    Id = jobId,
                    Status = ExportJobStatus.Queued,
                    CreatedDate = DateTime.UtcNow,
                    NumberOfObservations = Convert.ToInt32(validateResult.Count),
                    Format = ExportFormat.GeoJson,
                    Description = description,
                    OutputFieldSet = filter?.Output?.FieldSet
                };

                if (userExports.Jobs == null) userExports.Jobs = new List<ExportJobInfo>();
                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}