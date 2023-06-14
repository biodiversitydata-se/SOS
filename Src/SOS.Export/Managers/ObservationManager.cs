using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Models.ZendTo;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Services.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.IO.DwcArchive;

namespace SOS.Export.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class ObservationManager : IObservationManager
    {
        private readonly IFilterManager _filterManager;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ICsvFileWriter _csvWriter;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly IDwcArchiveEventFileWriter _dwcArchiveEventFileWriter;
        private readonly IExcelFileWriter _excelWriter;
        private readonly IGeoJsonFileWriter _geoJsonWriter;
        private readonly string _exportPath;
        private readonly IFileService _fileService;
        private readonly ILogger<ObservationManager> _logger;
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly IProcessInfoRepository _processInfoRepository;
        private readonly IZendToService _zendToService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwcArchiveFileWriter"></param>
        /// <param name="dwcArchiveEventFileWriter"></param>
        /// <param name="excelWriter"></param>
        /// <param name="geoJsonWriter"></param>
        /// <param name="csvWriter"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="blobStorageService"></param>
        /// <param name="zendToService"></param>
        /// <param name="fileDestination"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ObservationManager(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IDwcArchiveEventFileWriter dwcArchiveEventFileWriter,
            IExcelFileWriter excelWriter,
            IGeoJsonFileWriter geoJsonWriter,
            ICsvFileWriter csvWriter,
            IProcessedObservationCoreRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IFileService fileService,
            IBlobStorageService blobStorageService,
            IZendToService zendToService,
            FileDestination fileDestination,
            IFilterManager filterManager,
            ILogger<ObservationManager> logger)
        {
            _dwcArchiveFileWriter =
                dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _dwcArchiveEventFileWriter = 
                dwcArchiveEventFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveEventFileWriter));
            _excelWriter =
                excelWriter ?? throw new ArgumentNullException(nameof(excelWriter));
            _geoJsonWriter =
                geoJsonWriter ?? throw new ArgumentNullException(nameof(geoJsonWriter));
            _csvWriter =
                csvWriter ?? throw new ArgumentNullException(nameof(csvWriter));
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _processInfoRepository =
                processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _zendToService = zendToService ?? throw new ArgumentNullException(nameof(zendToService));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _exportPath = fileDestination?.Path ?? throw new ArgumentNullException(nameof(fileDestination));
            
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Make sure we are working with live data
            _processedObservationRepository.LiveMode = true;
        }

        /// <inheritdoc />
        public async Task<ZendToResponse> ExportAndSendAsync(
            int? roleId, 
            string authorizationApplicationIdentifier,
            SearchFilter filter, 
            string emailAddress,
            string description,
            ExportFormat exportFormat,
            string culture,
            bool flatOut,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            bool sensitiveObservations,
            bool sendMailFromZendTo,
            string encryptPassword,
            IJobCancellationToken cancellationToken)
        {
            FileExportResult fileExportResult = null;
            try
            {
                await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);

                fileExportResult = exportFormat switch
                {
                    ExportFormat.Csv => await CreateCsvExportAsync(filter, Guid.NewGuid().ToString(), culture, propertyLabelType, cancellationToken),
                    ExportFormat.Excel => await CreateExcelExportAsync(filter, Guid.NewGuid().ToString(), culture, propertyLabelType, cancellationToken),
                    ExportFormat.GeoJson => await CreateGeoJsonExportAsync(filter, Guid.NewGuid().ToString(), culture, flatOut, propertyLabelType, excludeNullValues, cancellationToken),
                    _ => await CreateDWCExportAsync(filter, Guid.NewGuid().ToString(), false, cancellationToken)
                };
                
                // zend file to user
                return await _zendToService.SendFile(
                    emailAddress, 
                    description, 
                    fileExportResult.FilePath, 
                    exportFormat, 
                    sendMailFromZendTo, 
                    !sensitiveObservations,// Can't include pass code if sensitive observations is selcted
                    sensitiveObservations || !string.IsNullOrEmpty(encryptPassword), // Encrypt file if sensitive observations is selcted or password is passed
                    encryptPassword);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export and send sightings");
                return new ZendToResponse();
            }
            finally
            {
                if (fileExportResult != null) _fileService.DeleteFile(fileExportResult.FilePath);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExportAndStoreAsync(SearchFilter filter, 
            string blobStorageContainer, 
            string fileName,
            string description,
            IJobCancellationToken cancellationToken)
        {
            return await ExportAndStoreAsync(filter, blobStorageContainer, fileName, null, description, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ExportAndStoreAsync(SearchFilter filter, 
            string blobStorageContainer, 
            string fileName,
            string emailAddress,
            string description,
            IJobCancellationToken cancellationToken)
        {
            FileExportResult fileExportResult = null;
            try
            {
                await _filterManager.PrepareFilterAsync(null, null, filter);
                fileExportResult = await CreateDWCExportAsync(filter, fileName, false, cancellationToken);

                // Blob Storage Containers must be in lower case
                blobStorageContainer = blobStorageContainer?.ToLower();

                // Make sure container exists
                await _blobStorageService.CreateContainerAsync(blobStorageContainer);

                // Upload file to blob storage
                var success = await _blobStorageService.UploadBlobAsync(fileExportResult.FilePath, blobStorageContainer);
           
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    // Send file to user
                    success = success && ((await _zendToService.SendFile(emailAddress, description, fileExportResult.FilePath, ExportFormat.DwC))?.Success ?? false);
                }

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export and store sightings");
                return false;
            }
            finally
            {
                // Remove local file
                if (fileExportResult != null) _fileService.DeleteFile(fileExportResult.FilePath);
            }
        }

        /// <summary>
        /// Create a csv export file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="culture"></param>
        /// <param name="propertyLabelType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<FileExportResult> CreateCsvExportAsync(
            SearchFilter filter,
            string fileName,
            string culture,
            PropertyLabelType propertyLabelType,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                bool gzip = true;
                var fileExportResult = await _csvWriter.CreateFileAync(
                    filter,
                    _exportPath,
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
        ///  Create a Darwin Core Archive file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="eventDwC"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<FileExportResult> CreateDWCExportAsync(SearchFilter filter, string fileName, bool eventDwC,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var processInfo = await _processInfoRepository.GetAsync(_processedObservationRepository.PublicIndexName);
                if (eventDwC)
                {
                    return await _dwcArchiveEventFileWriter.CreateEventDwcArchiveFileAsync(
                       DataProvider.FilterSubsetDataProvider,
                       filter,
                       fileName,
                       _processedObservationRepository,
                       processInfo,
                       _exportPath,
                       cancellationToken);
                }
               
                var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();
                return await _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                    DataProvider.FilterSubsetDataProvider,
                    filter,
                    fileName,
                    _processedObservationRepository,
                    fieldDescriptions,
                    processInfo,
                    _exportPath,
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

        private async Task<FileExportResult> CreateExcelExportAsync(SearchFilter filter, 
            string fileName, 
            string culture,
            PropertyLabelType propertyLabelType,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var fileExportResult = await _excelWriter.CreateFileAync(
                    filter,
                    _exportPath,
                    fileName,
                    culture,
                    propertyLabelType,
                    gzip: true,
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

        private async Task<FileExportResult> CreateGeoJsonExportAsync(SearchFilter filter, 
            string fileName, 
            string culture, 
            bool flatOut,
            PropertyLabelType propertyLabelType,
            bool excludeNullValues,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                bool gzip = true;
                var fileExportResult = await _geoJsonWriter.CreateFileAync(
                   filter,
                   _exportPath,
                   fileName,
                   culture,
                   flatOut,
                   propertyLabelType, 
                   excludeNullValues,
                   gzip,
                   cancellationToken);
                
                return fileExportResult;
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
    }
}