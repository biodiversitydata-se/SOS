using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    /// Manager for exports
    /// </summary>
    public class ExportManager : IExportManager
    {
        private readonly IFilterManager _filterManager;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly IDwcArchiveEventFileWriter _dwcArchiveEventFileWriter;
        private readonly ICsvFileWriter _csvWriter;
        private readonly IExcelFileWriter _excelWriter;
        private readonly IGeoJsonFileWriter _geoJsonWriter;
        private readonly ILogger<ExportManager> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProcessInfoRepository _processInfoRepository;

        /// <summary>
        /// Create an Csv file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="exportPath"></param>
        /// <param name="fileName"></param>
        /// <param name="culture"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="gzip"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<FileExportResult> CreateCsvExportAsync(SearchFilter filter, 
            string exportPath,
            string fileName, 
            string culture,
            PropertyLabelType propertyLabelType,
            bool gzip,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var fileExportResult = await _csvWriter.CreateFileAync(
                    filter,
                    exportPath,
                    fileName,
                    culture,
                    propertyLabelType,
                    gzip,
                    cancellationToken);

                return fileExportResult;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export sightings to Csv was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export sightings to Csv");
                throw;
            }
        }

        /// <summary>
        /// Create a Darwin Core Archive file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="exportPath"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="eventDwC"></param>
        /// <returns></returns>
        private async Task<FileExportResult> CreateDWCExportAsync(SearchFilter filter, string exportPath, string fileName,
            IJobCancellationToken cancellationToken,
            bool eventDwC = false)
        {
            try
            {
                var processInfo = await _processInfoRepository.GetAsync(_processedObservationRepository.PublicIndexName);

                if (eventDwC)
                {
                    filter.Output.SortOrders = new[] { new SortOrderFilter { SortBy = "event.eventId", SortOrder = SearchSortOrder.Asc } };
                    return await _dwcArchiveEventFileWriter.CreateEventDwcArchiveFileAsync(
                       DataProvider.FilterSubsetDataProvider,
                       filter,
                       fileName,
                       _processedObservationRepository,
                       processInfo,
                       exportPath,
                       cancellationToken);
                }
                var propertyFields =
                   ObservationPropertyFieldDescriptionHelper.GetExportFieldsFromOutputFields(filter?.Output?.Fields);
                var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions().
                   Where(fd => propertyFields.Select(pf => pf.DwcIdentifier.ToLower()).Contains(fd.DwcIdentifier.ToLower()));

                return await _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                    DataProvider.FilterSubsetDataProvider,
                    filter,
                    fileName,
                    _processedObservationRepository,
                    fieldDescriptions,
                    processInfo,
                    exportPath,
                    cancellationToken);
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export sightings was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export sightings");
                throw;
            }
        }

        /// <summary>
        /// Create an Excel file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="exportPath"></param>
        /// <param name="fileName"></param>
        /// <param name="culture"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="gzip"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<FileExportResult> CreateExcelExportAsync(SearchFilter filter, 
            string exportPath, 
            string fileName, 
            string culture,
            PropertyLabelType propertyLabelType,
            bool gzip,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var fileExportResult = await _excelWriter.CreateFileAync(
                    filter, 
                    exportPath,
                    fileName,
                    culture,
                    propertyLabelType,
                    gzip,
                    cancellationToken);

                return fileExportResult;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export sightings to Excel was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export sightings to Excel");
                throw;
            }
        }

        /// <summary>
        /// Create a GeoJson file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="exportPath"></param>
        /// <param name="fileName"></param>
        /// <param name="culture"></param>
        /// <param name="flatOut"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="excludeNullValues"></param>
        /// <param name="gzip"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<FileExportResult> CreateGeoJsonExportAsync(SearchFilter filter, 
            string exportPath, 
            string fileName, 
            string culture, 
            bool flatOut,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            bool gzip,
            IJobCancellationToken cancellationToken) 
        {
            try
            {
                var filePath = await _geoJsonWriter.CreateFileAync(
                   filter,
                   exportPath,
                   fileName,
                   culture,
                   flatOut,
                   propertyLabelType, 
                   excludeNullValues,
                   gzip,
                   cancellationToken);

                return filePath;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export sightings to GeoJson was canceled.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export sightings to GeoJson");
                throw;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="csvWriter"></param>
        /// <param name="dwcArchiveFileWriter"></param>
        /// <param name="dwcArchiveEventFileWriter"></param>
        /// <param name="excelWriter"></param>
        /// <param name="geoJsonWriter"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExportManager(
            ICsvFileWriter csvWriter,
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IDwcArchiveEventFileWriter dwcArchiveEventFileWriter,
            IExcelFileWriter excelWriter,
            IGeoJsonFileWriter geoJsonWriter,
            IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IFilterManager filterManager,
            ILogger<ExportManager> logger)
        {
            _csvWriter = csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            _dwcArchiveFileWriter =
                dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _dwcArchiveEventFileWriter =
                dwcArchiveEventFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveEventFileWriter));
            _excelWriter =
                excelWriter ?? throw new ArgumentNullException(nameof(excelWriter));
            _geoJsonWriter =
                geoJsonWriter ?? throw new ArgumentNullException(nameof(geoJsonWriter));

            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _processInfoRepository =
                processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<FileExportResult> CreateExportFileAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            ExportFormat exportFormat,
            string exportPath,
            string culture,
            bool flatOut,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            bool gzip,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);

                var fileExportResult = exportFormat switch
                {
                    ExportFormat.Csv => await CreateCsvExportAsync(filter, exportPath, Guid.NewGuid().ToString(), culture, propertyLabelType, gzip, cancellationToken),
                    ExportFormat.DwCEvent => await CreateDWCExportAsync(filter, exportPath, Guid.NewGuid().ToString(), cancellationToken, true),
                    ExportFormat.Excel => await CreateExcelExportAsync(filter, exportPath, Guid.NewGuid().ToString(), culture, propertyLabelType, gzip, cancellationToken),
                    ExportFormat.GeoJson => await CreateGeoJsonExportAsync(filter, exportPath, Guid.NewGuid().ToString(), culture, flatOut, propertyLabelType, excludeNullValues, gzip, cancellationToken),
                    _ => await CreateDWCExportAsync(filter, exportPath, Guid.NewGuid().ToString(), cancellationToken)
                };
                
                return fileExportResult;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create export file");
                return null;
            }
        }
    }
}
