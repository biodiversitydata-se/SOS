using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Export.Helpers;
using SOS.Export.IO.DwcArchive.Interfaces;
using SOS.Export.Models;
using SOS.Export.Repositories.Interfaces;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Models.Search;

namespace SOS.Export.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class SightingFactory : Interfaces.ISightingFactory
    {
        private readonly IProcessedSightingRepository _processedSightingRepository;
        private readonly IProcessInfoRepository _processInfoRepository;
        private readonly IFileService _fileService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IZendToService _zendToService;
        private readonly string _exportPath;
        private readonly IDwcArchiveFileWriter _dwcArchiveFileWriter;
        private readonly ILogger<SightingFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dwcArchiveFileWriter"></param>
        /// <param name="processedSightingRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="fileService"></param>
        /// <param name="zendToService"></param>
        /// <param name="fileDestination"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IProcessedSightingRepository processedSightingRepository,
            IProcessInfoRepository processInfoRepository,
            IFileService fileService,
            IBlobStorageService blobStorageService,
            IZendToService zendToService,
            FileDestination fileDestination,

            ILogger<SightingFactory> logger)
        {
            _processedSightingRepository = processedSightingRepository ?? throw new ArgumentNullException(nameof(processedSightingRepository));
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _zendToService = zendToService ?? throw new ArgumentNullException(nameof(zendToService));
            _exportPath = fileDestination?.Path ?? throw new ArgumentNullException(nameof(fileDestination));
            _dwcArchiveFileWriter = dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<string> CreateDWCExportAsync(ExportFilter filter, string fileName, IJobCancellationToken cancellationToken)
        {
            try
            {
                var processInfo = await _processInfoRepository.GetAsync(_processInfoRepository.ActiveInstance);
                
                var zipFilePath = await _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                    filter,
                    fileName,
                    _processedSightingRepository,
                    FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
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
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExportDWCAsync(ExportFilter filter, IJobCancellationToken cancellationToken)
        {
            var zipFilePath = "";
            try
            {
                var fileName = Guid.NewGuid().ToString();
                zipFilePath = await CreateDWCExportAsync(filter, fileName, cancellationToken);

                // Make sure container exists
                var container = $"sos-{DateTime.Now.Year}";
                await _blobStorageService.CreateContainerAsync(container);

                // Upload file to blob storage
                return await _blobStorageService.UploadBlobAsync(zipFilePath, container);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export sightings");
                return false;
            }
            finally
            {
                // Remove local file
                _fileService.DeleteFile(zipFilePath);
            }
        }

        public async Task<bool> ExportDWCAsync(ExportFilter filter, string emailAddress,
            IJobCancellationToken cancellationToken)
        {
            var zipFilePath = "";
            try
            {
                var fileName = Guid.NewGuid().ToString();
                zipFilePath = await CreateDWCExportAsync(filter, fileName, cancellationToken);

                // zend file to user
                return await _zendToService.SendFile(emailAddress, JsonConvert.SerializeObject(filter), zipFilePath);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to export sightings");
                return false;
            }
            finally
            {
                _fileService.DeleteFile(zipFilePath);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExportAllAsync(IJobCancellationToken cancellationToken)
        {
            return await ExportAllAsync(
                FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(), 
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ExportAllAsync(
            IEnumerable<FieldDescription> fieldDescriptions, 
            IJobCancellationToken cancellationToken)
        {
            return await ExportDWCAsync(new ExportFilter(), cancellationToken);
        }
    }
}