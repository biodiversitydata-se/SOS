using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.Managers
{
    /// <summary>
    ///     Observation manager class
    /// </summary>
    public class ObservationManager : IObservationManager
    {
        private readonly IFilterManager _filterManager;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly IExcelFileWriter _excelWriter;
        private readonly IGeoJsonFileWriter _geoJsonWriter;
        private readonly string _exportPath;
        private readonly IFileService _fileService;
        private readonly ILogger<ObservationManager> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProcessInfoRepository _processInfoRepository;
        private readonly IZendToService _zendToService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwcArchiveFileWriter"></param>
        /// <param name="excelWriter"></param>
        /// <param name="geoJsonWriter"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="blobStorageService"></param>
        /// <param name="zendToService"></param>
        /// <param name="fileDestination"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        public ObservationManager(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IExcelFileWriter excelWriter,
            IGeoJsonFileWriter geoJsonWriter,
            IProcessedObservationRepository processedObservationRepository,
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
            _excelWriter =
                excelWriter ?? throw new ArgumentNullException(nameof(excelWriter));
            _geoJsonWriter =
                geoJsonWriter ?? throw new ArgumentNullException(nameof(geoJsonWriter));

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
        public async Task<bool> ExportAndSendAsync(SearchFilter filter, 
            string emailAddress,
            string description,
            ExportFormat exportFormat,
            string culture,
            bool flatOut,
            IJobCancellationToken cancellationToken)
        {
            var zipFilePath = "";
            try
            {
                await _filterManager.PrepareFilter(null, filter);

                zipFilePath = exportFormat switch
                {
                    ExportFormat.DwC => await CreateDWCExportAsync(filter, Guid.NewGuid().ToString(), cancellationToken),
                    ExportFormat.Excel => await CreateExcelExportAsync(filter, Guid.NewGuid().ToString(), culture, cancellationToken),
                    ExportFormat.GeoJson => await CreateGeoJsonExportAsync(filter, Guid.NewGuid().ToString(), culture, flatOut, cancellationToken)
                };
                
                // zend file to user
                return await _zendToService.SendFile(emailAddress, description, zipFilePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export and send sightings");
                return false;
            }
            finally
            {
                _fileService.DeleteFile(zipFilePath);
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
            var zipFilePath = "";
            try
            {
                await _filterManager.PrepareFilter(null, filter);
                zipFilePath = await CreateDWCExportAsync(filter, fileName, cancellationToken);

                // Blob Storage Containers must be in lower case
                blobStorageContainer = blobStorageContainer?.ToLower();

                // Make sure container exists
                await _blobStorageService.CreateContainerAsync(blobStorageContainer);

                var tasks = new List<Task<bool>>
                {
                    // Upload file to blob storage
                    _blobStorageService.UploadBlobAsync(zipFilePath, blobStorageContainer),
                };

                if (!string.IsNullOrEmpty(emailAddress))
                {
                    // Send file to user
                    tasks.Add(_zendToService.SendFile(emailAddress, description, zipFilePath));
                }

                await Task.WhenAll(tasks);

                var success = tasks.All(t => t.Result);

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
                _fileService.DeleteFile(zipFilePath);
            }
        }

        /// <summary>
        ///     Create a Darwin Core Archive file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Path to created file</returns>
        private async Task<string> CreateDWCExportAsync(SearchFilter filter, string fileName,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var processInfo = await _processInfoRepository.GetAsync(_processedObservationRepository.PublicIndexName);
                var fieldDescriptions = FieldDescriptionHelper.GetAllDwcOccurrenceCoreFieldDescriptions();
                var zipFilePath = await _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                    DataProvider.FilterSubsetDataProvider,
                    filter,
                    fileName,
                    _processedObservationRepository,
                    fieldDescriptions,
                    processInfo,
                    _exportPath,
                    cancellationToken);
                cancellationToken?.ThrowIfCancellationRequested();

                return zipFilePath;
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

        private async Task<string> CreateExcelExportAsync(SearchFilter filter, 
            string fileName, string culture,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var zipFilePath = await _excelWriter.CreateFileAync(
                    filter,
                    _exportPath,
                    fileName,
                    culture,
                    cancellationToken);

                return zipFilePath;
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

        private async Task<string> CreateGeoJsonExportAsync(SearchFilter filter, 
            string fileName, string culture, bool flatOut,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var zipFilePath = await _geoJsonWriter.CreateFileAync(
                   filter,
                   _exportPath,
                   fileName,
                   culture,
                   flatOut,
                    cancellationToken);
                
                return zipFilePath;
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