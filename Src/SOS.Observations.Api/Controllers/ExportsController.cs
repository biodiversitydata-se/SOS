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
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Exceptions;
using SOS.Lib.Helpers;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Swagger;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Dtos.Export;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Extensions;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Exports controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ExportsController : ObservationBaseController
    {
        private readonly IBlobStorageManager _blobStorageManager;
        private readonly ICryptoService _cryptoService;
        private readonly IExportManager _exportManager;
        private readonly IFileService _fileService;
        private readonly IUserExportRepository _userExportRepository;
        private readonly int _defaultUserExportLimit;
        private readonly long _orderExportObservationsLimit;
        private readonly long _downloadExportObservationsLimit;
        private readonly string _exportPath;
        private readonly ILogger<ExportsController> _logger;

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

        private void HandleOutputFieldSet(SearchFilterInternalDto filter,
              OutputFieldSet outputFieldSet)
        {
            if (outputFieldSet != OutputFieldSet.None)
            {
                filter = filter ?? new SearchFilterInternalDto();
                filter.Output = filter?.Output ?? new OutputFilterExtendedDto();

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
        /// <param name="validateSearchFilter"></param>
        /// <param name="email"></param>
        /// <param name="userExport"></param>
        /// <param name="protectionFilter"></param>
        /// <param name="sendMailFromZendTo"></param>
        /// <param name="encryptPassword"></param>
        /// <param name="confirmEncryptPassword"></param>
        /// <returns></returns>
        private async Task<(IActionResult Result, long? Count)> OrderValidateAsync(
            SearchFilterBaseDto filter,
            bool validateSearchFilter,
            string email, 
            UserExport userExport,
            ProtectionFilterDto? protectionFilter,
            bool sendMailFromZendTo,
            string encryptPassword,
            string confirmEncryptPassword)
        {
            var validationResults = Result.Combine(
                validateSearchFilter ? ValidateSearchFilter(filter, allowObjectInOutputFields: false) : Result.Success(),
                ValidateBoundingBox(filter?.Geographics?.BoundingBox, false),
                ValidateEmail(email),
                ValidateUserExport(userExport),
                ValidateEncryptPassword(encryptPassword, confirmEncryptPassword, protectionFilter ?? ProtectionFilterDto.Public),
                !protectionFilter.Equals(ProtectionFilterDto.Public) && sendMailFromZendTo ? Result.Failure("You are not allowed to send e-mail from ZendTo when sensitive observations are requested") : Result.Success()
           );

            if (validationResults.IsFailure)
            {
                return (Result: BadRequest(validationResults.Error), Count: null);
            }

            var exportFilter = filter.ToSearchFilter(UserId, protectionFilter ?? ProtectionFilterDto.Public, "en-GB");
            var matchCount = await ObservationManager.GetMatchCountAsync(0, null, exportFilter);
            
            if (matchCount == 0)
            {
                return (Result: NoContent(), Count: 0);
            }

            if (matchCount > _orderExportObservationsLimit)
            {
                return (Result: BadRequest($"Query exceeds limit of {_orderExportObservationsLimit} observations."), Count: matchCount);
            }
            LogObservationCount(matchCount);
            return (Result: new OkObjectResult(exportFilter), Count: matchCount);            
        }

        /// <summary>
        /// Validate input for download request
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="validateSearchFilter"></param>
        /// <param name="protectionFilter"></param>
        /// <returns></returns>
        private async Task<IActionResult> DownloadValidateAsync(SearchFilterBaseDto filter, bool validateSearchFilter, ProtectionFilterDto? protectionFilter)
        {
            var validationResults = Result.Combine(
                validateSearchFilter ? ValidateSearchFilter(filter, allowObjectInOutputFields: false) : Result.Success(),
                ValidateBoundingBox(filter?.Geographics?.BoundingBox, false)
            );
           
            if (validationResults.IsFailure)
            {
                return BadRequest(validationResults);
            }

            var exportFilter = filter.ToSearchFilter(UserId, protectionFilter ?? ProtectionFilterDto.Public, "en-GB");
            var matchCount = await ObservationManager.GetMatchCountAsync(0, null, exportFilter);

            if (matchCount == 0)
            {
                return NoContent();
            }

            if (matchCount > _downloadExportObservationsLimit)
            {
                return BadRequest($"Query exceeds limit of {_downloadExportObservationsLimit} observations.");
            }
            LogObservationCount(matchCount);
            return new OkObjectResult(exportFilter);
        }

        /// <summary>
        /// Validate user export
        /// </summary>
        /// <param name="userExport"></param>
        /// <returns></returns>
        private Result ValidateUserExport(UserExport userExport)
        {
            var onGoingJobCount = userExport?.Jobs?.Where(j => new[] { ExportJobStatus.Queued, ExportJobStatus.Processing }.Contains(j.Status))?.Count() ?? 0;
            if (onGoingJobCount > (userExport?.Limit ?? 1))
            {
                return Result.Failure($"User already has {onGoingJobCount} on going exports.");
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
        /// <param name="cryptoService"></param>
        /// <param name="fileService"></param>
        /// <param name="userExportRepository"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExportsController(IObservationManager observationManager,
            IBlobStorageManager blobStorageManager,
            IAreaManager areaManager,
            ITaxonManager taxonManager,
            IExportManager exportManager,
            ICryptoService cryptoService,
            IFileService fileService,
            IUserExportRepository userExportRepository,
            ObservationApiConfiguration configuration,
            ILogger<ExportsController> logger) : base(observationManager, areaManager, taxonManager, configuration)
        {
            _blobStorageManager = blobStorageManager ?? throw new ArgumentNullException(nameof(blobStorageManager));
            _exportManager = exportManager ?? throw new ArgumentNullException(nameof(exportManager));
            _cryptoService = cryptoService ?? throw new ArgumentNullException(nameof(cryptoService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _userExportRepository =
                userExportRepository ?? throw new ArgumentNullException(nameof(userExportRepository));
            _defaultUserExportLimit = configuration?.DefaultUserExportLimit ?? throw new ArgumentNullException(nameof(configuration));
            _orderExportObservationsLimit = configuration.OrderExportObservationsLimit;
            _downloadExportObservationsLimit = configuration.DownloadExportObservationsLimit;
            _exportPath = configuration.ExportPath;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Returns a list of data provider datasets (DwC-A) available for download. 
        /// A file is usually created once a day for each dataset.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Datasets")]
        [ProducesResponseType(typeof(IEnumerable<Lib.Models.Misc.File>), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetDatasetsListAsync()
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

        /// <summary>
        /// Get all exports for a user
        /// </summary>
        /// <returns></returns>
        [HttpGet("My")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(IEnumerable<ExportJobInfoDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMyExportsAsync()
        {
            try
            {
                var userExport = await GetUserExportsAsync();

                if (!userExport?.Jobs?.Any() ?? true)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return new OkObjectResult(userExport?.Jobs?.Select(j => j.ToDto())?.OrderByDescending(j => j.CreatedDate));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting export files");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get export by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("My/{id}")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(ExportJobInfoDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMyExportAsync([FromRoute] string id)
        {
            try
            {
                var userExport = await GetUserExportsAsync();
                var job = userExport?.Jobs?.Where(j => j.Id.Equals(id))?.FirstOrDefault();
                if (job == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return new OkObjectResult(job.ToDto());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting export files");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        ///  Download Csv export file. The limit is 25 000 observations. If you need to download more observations, use the OrderCsv endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        [HttpPost("Download/Csv")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DownloadCsvAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool gzip = true,
            [FromQuery] bool sensitiveObservations = false)
        {
            FileExportResult fileExportResult = null;
            try
            {
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(protectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
               
                var validateResult = await DownloadValidateAsync(filter, validateSearchFilter, protectionFilter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
               
                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        exportFilter,
                        ExportFormat.Csv,
                        _exportPath, cultureCode,
                        false,
                        propertyLabelType,
                        false,
                        gzip,
                        JobCancellationToken.Null);

                if (gzip)
                    return GetFile(fileExportResult.FilePath, "Observations_Csv.zip", "application/zip");
                else
                    return GetFile(fileExportResult.FilePath, "Observations.csv", "text/tab-separated-values"); // or "text/csv"?                
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
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

        /// <summary>
        /// Download DwC export file. The limit is 25 000 observations. If you need to download more observations, use the OrderDwC endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter"></param>
        /// <param name="eventBased">Event based Darwin Core if true</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        [HttpPost("Download/DwC")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> DownloadDwCAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter,
            [FromQuery] bool eventBased = false,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            FileExportResult fileExportResult = null;

            try
            {
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(protectionFilter);

                var validateResult = await DownloadValidateAsync(filter, validateSearchFilter, protectionFilter);
                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        exportFilter,
                        eventBased ? ExportFormat.DwCEvent : ExportFormat.DwC,
                        _exportPath, 
                        Cultures.en_GB, 
                        false,
                        PropertyLabelType.PropertyPath,
                        false,
                        true,
                        JobCancellationToken.Null);

                if (fileExportResult == null)
                {
                    return new StatusCodeResult((int)HttpStatusCode.NoContent);
                }

                return GetFile(fileExportResult.FilePath, "Observations_DwC.zip", "application/zip");
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
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

        /// <summary>
        ///  Download Excel export file. The limit is 25 000 observations. If you need to download more observations, use the OrderExcel endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        [HttpPost("Download/Excel")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> DownloadExcelAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName, 
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool gzip = true,
            [FromQuery] bool sensitiveObservations = false)
        {
            FileExportResult fileExportResult = null;

            try
            {
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(protectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                
                var validateResult = await DownloadValidateAsync(filter, validateSearchFilter, protectionFilter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        exportFilter,
                        ExportFormat.Excel, 
                        _exportPath, 
                        cultureCode,
                        false,
                        propertyLabelType,
                        false,
                        gzip,
                        JobCancellationToken.Null);

                if (gzip)
                    return GetFile(fileExportResult.FilePath, "Observations_Excel.zip", "application/zip");
                else
                    return GetFile(fileExportResult.FilePath, "Observations.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
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

        /// <summary>
        /// Download GeoJson export file. The limit is 25 000 observations. If you need to download more observations, use the OrderGeoJson endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The label type to use if flat=false.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="flat">If true, the observations will be serialized as a flat JSON structure.</param>
        /// <param name="excludeNullValues">Exclude properties with null values. Applies when <paramref name="flat"/>=true.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        [HttpPost("Download/GeoJson")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]        
        public async Task<IActionResult> DownloadGeoJsonAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool flat = true,
            [FromQuery] bool excludeNullValues = false,
            [FromQuery] bool gzip = true,
            [FromQuery] bool sensitiveObservations = false)
        {
            FileExportResult fileExportResult = null;

            try
            {
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);

                HandleOutputFieldSet(filter, outputFieldSet);
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                
                var validateResult = await DownloadValidateAsync(filter, validateSearchFilter, protectionFilter);
                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;

                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        exportFilter, 
                        ExportFormat.GeoJson, 
                        _exportPath, 
                        cultureCode, 
                        flat,
                        propertyLabelType,
                        excludeNullValues,
                        gzip,
                        JobCancellationToken.Null);

                if (gzip)                
                    return GetFile(fileExportResult.FilePath, "Observations_GeoJson.zip", "application/zip");
                else                
                    return GetFile(fileExportResult.FilePath, "Observations.geojson", "application/geo+json");
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
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

        /// <summary>
        /// Starts the process of creating a Csv file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        [HttpPost("Order/Csv")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> OrderCsvAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter,
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "",
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(protectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, validateSearchFilter, UserEmail, userExports, protectionFilter, sendMailFromZendTo, encryptPassword, confirmEncryptPassword);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }
                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, roleId, authorizationApplicationIdentifier, UserEmail, description, ExportFormat.Csv, cultureCode, false,
                        propertyLabelType, false, sensitiveObservations, sendMailFromZendTo, encryptedPassword, null, JobCancellationToken.Null));

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

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Starts the process of creating a DwC-A file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="eventBased">Event based Darwin Core if true</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <returns></returns>
        [HttpPost("Order/DwC")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> OrderDwCAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter, 
            [FromQuery] string description,
            [FromQuery] bool eventBased = false,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "")
        {
            try
            {
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(protectionFilter);

                var userExports = await GetUserExportsAsync();
                var validateResult = await OrderValidateAsync(filter, validateSearchFilter, UserEmail, userExports, protectionFilter, sendMailFromZendTo, encryptPassword, confirmEncryptPassword);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, roleId, authorizationApplicationIdentifier, UserEmail, description, eventBased ? ExportFormat.DwCEvent : ExportFormat.DwC, "en-GB", false,
                        PropertyLabelType.PropertyName, false, sensitiveObservations, sendMailFromZendTo, encryptedPassword, null, JobCancellationToken.Null));

                var exportJobInfo = new ExportJobInfo
                {
                    Id = jobId,
                    Status = ExportJobStatus.Queued,
                    CreatedDate = DateTime.UtcNow,
                    NumberOfObservations = Convert.ToInt32(validateResult.Count),
                    Format = eventBased ? ExportFormat.DwCEvent : ExportFormat.DwC,
                    Description = description,
                    OutputFieldSet = filter?.Output?.FieldSet
                };

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int) HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Starts the process of creating a Excel file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        [HttpPost("Order/Excel")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> OrderExcelAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter, 
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "",
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);

                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, validateSearchFilter, UserEmail, userExports, protectionFilter, sendMailFromZendTo, encryptPassword, confirmEncryptPassword);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);

                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, roleId, authorizationApplicationIdentifier, UserEmail, description, ExportFormat.Excel, cultureCode, false,
                        propertyLabelType, false, sensitiveObservations, sendMailFromZendTo, encryptedPassword, null, JobCancellationToken.Null));

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

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Starts the process of creating a GeoJSON file with observations based on the provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="description">A description of your download. Will be displayed in the email.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="propertyLabelType">The label type to use if flat=false.</param>
        /// <param name="flat">If true, the observations will be serialized as a flat JSON structure.</param>
        /// <param name="excludeNullValues">Exclude properties with null values. Applies when <paramref name="flat"/>=true.</param>
        /// <param name="cultureCode">The culture code used for translation vocabulary values.</param>
        /// <returns></returns>
        [HttpPost("Order/GeoJson")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> OrderGeoJsonAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterDto filter, 
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "",
            [FromQuery] bool flat = true,
            [FromQuery] bool excludeNullValues = false,
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                // SearchFilterDto don't support protection filter, declare it localy
                var protectionFilter = sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public;
                CheckAuthorization(protectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, validateSearchFilter, UserEmail, userExports, protectionFilter, sendMailFromZendTo, encryptPassword, confirmEncryptPassword);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, roleId, authorizationApplicationIdentifier, UserEmail, description, ExportFormat.GeoJson, cultureCode,
                        flat, propertyLabelType, excludeNullValues, sensitiveObservations, sendMailFromZendTo, encryptedPassword, null, JobCancellationToken.Null));

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

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        #region Internal
        /// <summary>
        ///  Download Csv export file. The limit is 25 000 observations. If you need to download more observations, use the OrderCsv endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        [HttpPost("Internal/Download/Csv")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> DownloadCsvInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool gzip = true,
            [FromQuery] bool sensitiveObservations = false)
        {
            FileExportResult fileExportResult = null;
            try
            {
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                CheckAuthorization(filter.ProtectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);

                var validateResult = await DownloadValidateAsync(filter, validateSearchFilter, filter.ProtectionFilter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;

                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        exportFilter,
                        ExportFormat.Csv,
                        _exportPath, cultureCode,
                        false,
                        propertyLabelType,
                        false,
                        gzip,
                        JobCancellationToken.Null);

                if (gzip)
                    return GetFile(fileExportResult.FilePath, "Observations_Csv.zip", "application/zip");
                else
                    return GetFile(fileExportResult.FilePath, "Observations.csv", "text/tab-separated-values"); // or "text/csv"?                
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
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

        /// <summary>
        /// Download DwC export file. The limit is 25 000 observations. If you need to download more observations, use the OrderDwC endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter"></param>
        /// <param name="eventBased">Event based Darwin Core if true</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        [HttpPost("Internal/Download/DwC")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> DownloadDwCInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] bool eventBased = false,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false)
        {
            FileExportResult fileExportResult = null;

            try
            {
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                CheckAuthorization(filter.ProtectionFilter);

                var validateResult = await DownloadValidateAsync(filter, validateSearchFilter, filter.ProtectionFilter);
                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        exportFilter,
                        eventBased ? ExportFormat.DwCEvent : ExportFormat.DwC,
                        _exportPath,
                        Cultures.en_GB,
                        false,
                        PropertyLabelType.PropertyPath,
                        false,
                        true,
                        JobCancellationToken.Null);

                return GetFile(fileExportResult.FilePath, "Observations_DwC.zip", "application/zip");
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error exporting DwC file");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                if (fileExportResult != null) _fileService.DeleteFile(fileExportResult.FilePath);
            }
        }

        /// <summary>
        ///  Download Excel export file. The limit is 25 000 observations. If you need to download more observations, use the OrderExcel endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        [HttpPost("Internal/Download/Excel")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> DownloadExcelInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool gzip = true,
            [FromQuery] bool sensitiveObservations = false)
        {
            FileExportResult fileExportResult = null;

            try
            {
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                CheckAuthorization(filter.ProtectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);

                var validateResult = await DownloadValidateAsync(filter, validateSearchFilter, filter.ProtectionFilter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;
                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        exportFilter,
                        ExportFormat.Excel,
                        _exportPath,
                        cultureCode,
                        false,
                        propertyLabelType,
                        false,
                        gzip,
                        JobCancellationToken.Null);

                if (gzip)
                    return GetFile(fileExportResult.FilePath, "Observations_Excel.zip", "application/zip");
                else
                    return GetFile(fileExportResult.FilePath, "Observations.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
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

        /// <summary>
        /// Download GeoJson export file. The limit is 25 000 observations. If you need to download more observations, use the OrderGeoJson endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The label type to use if flat=false.</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <param name="flat">If true, the observations will be serialized as a flat JSON structure.</param>
        /// <param name="excludeNullValues">Exclude properties with null values. Applies when <paramref name="flat"/>=true.</param>
        /// <param name="gzip">If true (default), the resulting file will be compressed by the GZIP file format.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <returns></returns>
        [HttpPost("Internal/Download/GeoJson")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [InternalApi]
        public async Task<IActionResult> DownloadGeoJsonInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] string cultureCode = "sv-SE",
            [FromQuery] bool flat = true,
            [FromQuery] bool excludeNullValues = false,
            [FromQuery] bool gzip = true,
            [FromQuery] bool sensitiveObservations = false)
        {
            FileExportResult fileExportResult = null;

            try
            {
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                CheckAuthorization(filter.ProtectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);

                var validateResult = await DownloadValidateAsync(filter, validateSearchFilter, filter.ProtectionFilter);
                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var exportFilter = (SearchFilter)okResult.Value;

                fileExportResult =
                    await _exportManager.CreateExportFileAsync(
                        roleId,
                        authorizationApplicationIdentifier,
                        exportFilter,
                        ExportFormat.GeoJson,
                        _exportPath,
                        cultureCode,
                        flat,
                        propertyLabelType,
                        excludeNullValues,
                        gzip,
                        JobCancellationToken.Null);

                if (gzip)
                    return GetFile(fileExportResult.FilePath, "Observations_GeoJson.zip", "application/zip");
                else
                    return GetFile(fileExportResult.FilePath, "Observations.geojson", "application/geo+json");
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
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

        /// <summary>
        /// Starts the process of creating a Csv file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        [HttpPost("Internal/Order/Csv")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [InternalApi]
        public async Task<IActionResult> OrderCsvInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "",
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                CheckAuthorization(filter.ProtectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, validateSearchFilter, UserEmail, userExports, filter.ProtectionFilter, sendMailFromZendTo, encryptPassword, confirmEncryptPassword);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, roleId, authorizationApplicationIdentifier, UserEmail, description, ExportFormat.Csv, cultureCode, false,
                        propertyLabelType, false, sensitiveObservations, sendMailFromZendTo, encryptedPassword, null, JobCancellationToken.Null));

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

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Starts the process of creating a DwC-A file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="eventBased">Event based Darwin Core if true</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <returns></returns>
        [HttpPost("Internal/Order/DwC")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [InternalApi]
        public async Task<IActionResult> OrderDwCInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] string description,
            [FromQuery] bool eventBased = false,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "")
        {
            try
            {
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                CheckAuthorization(filter.ProtectionFilter);

                var userExports = await GetUserExportsAsync();
                var validateResult = await OrderValidateAsync(filter, validateSearchFilter, UserEmail, userExports, filter.ProtectionFilter, sendMailFromZendTo, encryptPassword, confirmEncryptPassword);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, roleId, authorizationApplicationIdentifier, UserEmail, description, eventBased ? ExportFormat.DwCEvent : ExportFormat.DwC, "en-GB", false,
                        PropertyLabelType.PropertyName, false, sensitiveObservations, sendMailFromZendTo, encryptedPassword, null, JobCancellationToken.Null));

                var exportJobInfo = new ExportJobInfo
                {
                    Id = jobId,
                    Status = ExportJobStatus.Queued,
                    CreatedDate = DateTime.UtcNow,
                    NumberOfObservations = Convert.ToInt32(validateResult.Count),
                    Format = eventBased ? ExportFormat.DwCEvent : ExportFormat.DwC,
                    Description = description,
                    OutputFieldSet = filter?.Output?.FieldSet
                };

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Starts the process of creating a Excel file with observations based on provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">Search filter.</param>
        /// <param name="description">A summary of the dataset you request. The description will be included in the email. If empty, an automatic description will be created.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="propertyLabelType">The column header type.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="cultureCode">The culture code used for translating vocabulary values.</param>
        /// <returns></returns>
        [HttpPost("Internal/Order/Excel")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [InternalApi]
        public async Task<IActionResult> OrderExcelInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "",
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                CheckAuthorization(filter.ProtectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, validateSearchFilter, UserEmail, userExports, filter.ProtectionFilter, sendMailFromZendTo, encryptPassword, confirmEncryptPassword);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, roleId, authorizationApplicationIdentifier, UserEmail, description, ExportFormat.Excel, cultureCode, false,
                        propertyLabelType, false, sensitiveObservations, sendMailFromZendTo, encryptedPassword, null, JobCancellationToken.Null));

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

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Starts the process of creating a GeoJSON file with observations based on the provided filter.
        /// When the file is ready, you will receive an email containing a download link.
        /// The limit is 2 000 000 observations.
        /// You can see the status of your export request by calling the "/Jobs/{jobId}/Status" endpoint.
        /// </summary>
        /// <param name="roleId">Limit user authorization too specified role</param>
        /// <param name="authorizationApplicationIdentifier">Name of application used in authorization.</param>
        /// <param name="filter">The search filter.</param>
        /// <param name="description">A description of your download. Will be displayed in the email.</param>
        /// <param name="outputFieldSet">Obsolete, will be overided by fieldset in body data if any. The observation property field set.</param>
        /// <param name="validateSearchFilter">If true, validation of search filter values will be made. I.e. HTTP bad request response will be sent if there are invalid parameter values.</param>
        /// <param name="sensitiveObservations">Include sensitive observations if true</param>
        /// <param name="sendMailFromZendTo">Send pick up file e-mail from ZendTo when file is reay to pick up (Only work if sensitiveObservations = false)</param>
        /// <param name="encryptPassword">Password used to encrypt file</param>
        /// <param name="confirmEncryptPassword">Confirm encrypt password</param>
        /// <param name="propertyLabelType">The label type to use if flat=false.</param>
        /// <param name="flat">If true, the observations will be serialized as a flat JSON structure.</param>
        /// <param name="excludeNullValues">Exclude properties with null values. Applies when <paramref name="flat"/>=true.</param>
        /// <param name="cultureCode">The culture code used for translation vocabulary values.</param>
        /// <returns></returns>
        [HttpPost("Internal/Order/GeoJson")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [InternalApi]
        public async Task<IActionResult> OrderGeoJsonInternalAsync(
            [FromHeader(Name = "X-Authorization-Role-Id")] int? roleId,
            [FromHeader(Name = "X-Authorization-Application-Identifier")] string authorizationApplicationIdentifier,
            [FromBody] SearchFilterInternalDto filter,
            [FromQuery] string description,
            [FromQuery] OutputFieldSet outputFieldSet = OutputFieldSet.None,
            [FromQuery] bool validateSearchFilter = false,
            [FromQuery] PropertyLabelType propertyLabelType = PropertyLabelType.PropertyName,
            [FromQuery] bool sensitiveObservations = false,
            [FromQuery] bool sendMailFromZendTo = true,
            [FromQuery] string encryptPassword = "",
            [FromQuery] string confirmEncryptPassword = "",
            [FromQuery] bool flat = true,
            [FromQuery] bool excludeNullValues = false,
            [FromQuery] string cultureCode = "sv-SE")
        {
            try
            {
                filter.ProtectionFilter ??= (sensitiveObservations ? ProtectionFilterDto.Sensitive : ProtectionFilterDto.Public);
                CheckAuthorization(filter.ProtectionFilter);

                HandleOutputFieldSet(filter, outputFieldSet);
                var userExports = await GetUserExportsAsync();
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter, validateSearchFilter, UserEmail, userExports, filter.ProtectionFilter, sendMailFromZendTo, encryptPassword, confirmEncryptPassword);

                if (validateResult.Result is not OkObjectResult okResult)
                {
                    return validateResult.Result;
                }

                var encryptedPassword = await _cryptoService.EncryptAsync(encryptPassword);
                var exportFilter = (SearchFilter)okResult.Value;
                var jobId = BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, roleId, authorizationApplicationIdentifier, UserEmail, description, ExportFormat.GeoJson, cultureCode,
                        flat, propertyLabelType, excludeNullValues, sensitiveObservations, sendMailFromZendTo, encryptedPassword, null, JobCancellationToken.Null));

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

                userExports.Jobs.Add(exportJobInfo);
                await UpdateUserExportsAsync(userExports);

                return new OkObjectResult(jobId);
            }
            catch (AuthenticationRequiredException)
            {
                return new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
        #endregion Internal
    }
}