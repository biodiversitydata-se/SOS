using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Managers.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Helpers;
using SOS.Lib.Models.DOI;
using SOS.Lib.Models.Search;
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
        private readonly IDOIRepository _doiRepository;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly string _exportPath;
        private readonly IFileService _fileService;
        private readonly ILogger<ObservationManager> _logger;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly IProcessInfoRepository _processInfoRepository;
        private readonly IZendToService _zendToService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="doiRepository"></param>
        /// <param name="dwcArchiveFileWriter"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="blobStorageService"></param>
        /// <param name="zendToService"></param>
        /// <param name="fileDestination"></param>
        /// <param name="filterManager"></param>
        /// <param name="logger"></param>
        public ObservationManager(IDOIRepository doiRepository,
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            IFileService fileService,
            IBlobStorageService blobStorageService,
            IZendToService zendToService,
            FileDestination fileDestination,
            IFilterManager filterManager,
            ILogger<ObservationManager> logger)
        {
            _doiRepository = doiRepository ?? throw new ArgumentNullException(nameof(doiRepository));
            _processedObservationRepository = processedObservationRepository ??
                                              throw new ArgumentNullException(nameof(processedObservationRepository));
            _processInfoRepository =
                processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _zendToService = zendToService ?? throw new ArgumentNullException(nameof(zendToService));
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _exportPath = fileDestination?.Path ?? throw new ArgumentNullException(nameof(fileDestination));
            _dwcArchiveFileWriter =
                dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ExportAndSendAsync(ExportFilter filter, string emailAddress,
            IJobCancellationToken cancellationToken)
        {
            var zipFilePath = "";
            try
            {
                var preparedFilter = await _filterManager.PrepareFilter(filter);
                zipFilePath = await CreateDWCExportAsync(preparedFilter, Guid.NewGuid().ToString(), cancellationToken);

                // zend file to user
                return await _zendToService.SendFile(emailAddress, JsonConvert.SerializeObject(filter), zipFilePath);
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
        public async Task<bool> ExportAndStoreAsync(ExportFilter filter, string blobStorageContainer, string fileName,
            bool isDOI, IJobCancellationToken cancellationToken)
        {
            var zipFilePath = "";
            try
            {
                var preparedFilter = await _filterManager.PrepareFilter(filter);
                zipFilePath = await CreateDWCExportAsync(preparedFilter, fileName, cancellationToken);

                // Blob Storage Containers must be in lower case
                blobStorageContainer = blobStorageContainer?.ToLower();

                // Make sure container exists
                await _blobStorageService.CreateContainerAsync(blobStorageContainer);

                // Upload file to blob storage
                var success = await _blobStorageService.UploadBlobAsync(zipFilePath, blobStorageContainer);

                // If upload was successful and it's a DOI
                if (success && isDOI)
                {
                    var doi = new DOI
                    {
                        Container = blobStorageContainer,
                        Id = Guid.Parse(fileName),
                        Filter = filter,
                        Created = DateTime.Now,
                        CreatedBy = "Todo"
                    };

                    return await _doiRepository.AddAsync(doi);
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
        private async Task<string> CreateDWCExportAsync(ExportFilter filter, string fileName,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                var processInfo = await _processInfoRepository.GetAsync(_processedObservationRepository.CollectionName);
                var fieldDescriptions = FieldDescriptionHelper.GetDwcFieldDescriptionsForTestingPurpose();

                var zipFilePath = await _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                    filter,
                    fileName,
                    _processedObservationRepository,
                    fieldDescriptions,
                    //FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
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
    }
}