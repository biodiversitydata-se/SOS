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
        private readonly long _exportObservationsLimit;
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

        private async Task<IActionResult> ValidateAsync(ExportFilterDto filter)
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

            if (matchCount > _exportObservationsLimit)
            {
                return BadRequest($"Query exceeds limit of {_exportObservationsLimit} observations.");
            }
            
            return new OkObjectResult((email, exportFilter));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="blobStorageManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public ExportsController(IObservationManager observationManager, 
            IBlobStorageManager blobStorageManager, 
            IAreaManager areaManager,
            ITaxonManager taxonManager, 
            ObservationApiConfiguration configuration, 
            ILogger<ExportsController> logger) :base(observationManager, areaManager, taxonManager)
        {
            _blobStorageManager = blobStorageManager ?? throw new ArgumentNullException(nameof(blobStorageManager));
            _exportObservationsLimit = configuration?.ExportObservationsLimit ?? throw new ArgumentNullException(nameof(configuration));
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
        [HttpPost("DwC")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportDwC([FromBody] ExportFilterDto filter, [FromQuery] string description)
        {
            try
            {
                var validateResult = await ValidateAsync(filter);

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
        [HttpPost("Excel")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportExcel([FromBody] ExportFilterDto filter, [FromQuery] string description, [FromQuery] ExportPropertySet exportPropertySet)
        {
            try
            {
                var validateResult = await ValidateAsync(filter);

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
        [HttpPost("GeoJson")]
        [Authorize/*(Roles = "Privat")*/]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ExportGeoJson([FromBody] ExportFilterDto filter, [FromQuery] string description, [FromQuery] ExportPropertySet exportPropertySet)
        {
            try
            {
                var validateResult = await ValidateAsync(filter);

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