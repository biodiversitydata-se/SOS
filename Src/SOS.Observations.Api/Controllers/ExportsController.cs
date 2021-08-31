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
using SOS.Lib.Helpers;
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
                "DatasetName",
                "Identification.Validated",
                "Identification.UncertainIdentification",
                "Location.County.Name",
                "Location.Municipality.Name",
                "Location.DecimalLongitude",
                "Location.DecimalLatitude",
                "Location.CoordinateUncertaintyInMeters",
                "Occurrence.OccurrenceStatus.Value",
                "Occurrence.RecordedBy",
                "Taxon.Attributes.OrganismGroup",
                "Taxon.ScientificName",
                "Taxon.VernacularName"
            };

            if (exportPropertySet == ExportPropertySet.Extended)
            {
                outputFields.AddRange(new []
                {
                    "CollectionCode",
                    "InstitutionCode",
                    "OwnerInstitutionCode",
                    "BasisOfRecord",
                    "Event.Habitat",
                    "Event.EventRemarks",
                    "Event.SamplingEffort",
                    "Event.SamplingProtocol",
                    "Event.SampleSizeUnit",
                    "Event.SampleSizeValue",
                    "Location.Locality",
                    "Location.Province.Name",
                    "Location.Parish.Name",
                    "Location.GeodeticDatum",
                    "Occurrence.ReportedBy",
                    "Occurrence.Url",
                    "Occurrence.AssociatedMedia",
                    "Occurrence.OccurrenceRemarks",
                    "Occurrence.Activity.Value",
                    "Occurrence.Behavior.Value",
                    "Occurrence.LifeStage.Value",
                    "Occurrence.ReproductiveCondition.Value",
                    "Occurrence.Sex.Value",
                    "Occurrence.Biotope.Value",
                    "Occurrence.BiotopeDescription",
                    "Occurrence.ProtectionLevel",
                    "Occurrence.IsNeverFoundObservation",
                    "Occurrence.IsNotRediscoveredObservation",
                    "Occurrence.IsNaturalOccurrence",
                    "Occurrence.IsPositiveObservation",
                    "Occurrence.Substrate.Name.Value",
                    "Occurrence.individualCount",
                    "Occurrence.OrganismQuantity",
                    "Occurrence.OrganismQuantityInt",
                    "Occurrence.OrganismQuantityUnit",
                    "Identification.ValidationStatus",
                    "Identification.ConfirmedBy",
                    "Identification.IdentifiedBy",
                    "Identification.VerifiedBy",
                    "Identification.DeterminationMethod.Value",
                    "Taxon.Kingdom",
                    "Taxon.Phylum",
                    "Taxon.Class",
                    "Taxon.Order",
                    "Taxon.Family",
                    "Taxon.Genus",
                    "Taxon.TaxonId",
                    "Taxon.Attributes.DyntaxaTaxonId",
                    "Taxon.Attributes.ProtectionLevel.Value",
                    "Taxon.Attributes.RedlistCategory",
                    "Taxon.Attributes.ProtectedByLaw",
                    "Project.Id",
                    "Project.Name"
                });
            }

            // Order fields by name
            outputFields = outputFields.OrderBy(s => s).ToList();

            // Make sure some selected fields occurs first
            outputFields.InsertRange(0, new []
            {
                "Occurrence.OccurrenceId",
                "Event.StartDate",
                "Event.EndDate"
            });

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
                    await _exportManager.CreateExportFileAsync(exportFilter, ExportFormat.DwC, _exportPath, Cultures.en_GB, false,
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
        public async Task<IActionResult> DownloadExcelAsync([FromBody] ExportFilterDto filter, [FromQuery] ExportPropertySet exportPropertySet, [FromQuery] string cultureCode)
        {
            cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
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
                    await _exportManager.CreateExportFileAsync(exportFilter, ExportFormat.Excel, 
                        _exportPath, cultureCode,
                        false,
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
        public async Task<IActionResult> DownloadGeoJsonAsync([FromBody] ExportFilterDto filter, 
            [FromQuery] ExportPropertySet exportPropertySet, [FromQuery] string cultureCode, [FromQuery] bool flatOut)
        {
            cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
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
                    await _exportManager.CreateExportFileAsync(exportFilter, ExportFormat.GeoJson, 
                        _exportPath, cultureCode, flatOut,
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
                    job.RunAsync(exportFilter, email, description, ExportFormat.DwC, "en-GB", false, JobCancellationToken.Null)));
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
        public async Task<IActionResult> OrderExcelAsync([FromBody] ExportFilterDto filter, [FromQuery] string description, 
            [FromQuery] ExportPropertySet exportPropertySet, [FromQuery] string cultureCode)
        {
            try
            {
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var (email, exportFilter) = ((string, SearchFilter))okResult.Value;
                PopulateOutputFields(exportFilter, exportPropertySet);

                return new OkObjectResult(BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, email, description, ExportFormat.Excel, cultureCode, false, JobCancellationToken.Null)));
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
        public async Task<IActionResult> OrderGeoJsonAsync([FromBody] ExportFilterDto filter, [FromQuery] string description, 
            [FromQuery] ExportPropertySet exportPropertySet, [FromQuery] string cultureCode, [FromQuery] bool flatOut)
        {
            try
            {
                cultureCode = CultureCodeHelper.GetCultureCode(cultureCode);
                var validateResult = await OrderValidateAsync(filter);

                if (validateResult is not OkObjectResult okResult)
                {
                    return validateResult;
                }

                var (email, exportFilter) = ((string, SearchFilter))okResult.Value;
                PopulateOutputFields(exportFilter, exportPropertySet);

                return new OkObjectResult(BackgroundJob.Enqueue<IExportAndSendJob>(job =>
                    job.RunAsync(exportFilter, email, description, ExportFormat.GeoJson, cultureCode, flatOut, JobCancellationToken.Null)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Running export failed");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}