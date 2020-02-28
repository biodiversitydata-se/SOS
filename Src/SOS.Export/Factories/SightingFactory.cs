using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
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
        /// <param name="blobStorageService"></param>
        /// <param name="fileDestination"></param>
        /// <param name="logger"></param>
        public SightingFactory(
            IDwcArchiveFileWriter dwcArchiveFileWriter,
            IProcessedSightingRepository processedSightingRepository,
            IProcessInfoRepository processInfoRepository,
            IFileService fileService,
            IBlobStorageService blobStorageService,
            FileDestination fileDestination,

            ILogger<SightingFactory> logger)
        {
            _processedSightingRepository = processedSightingRepository ?? throw new ArgumentNullException(nameof(processedSightingRepository));
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _exportPath = fileDestination?.Path ?? throw new ArgumentNullException(nameof(fileDestination));
            _dwcArchiveFileWriter = dwcArchiveFileWriter ?? throw new ArgumentNullException(nameof(dwcArchiveFileWriter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> ExportDWCAsync(AdvancedFilter filter, IJobCancellationToken cancellationToken)
        {
            string zipFilePath = null;

            try
            {
                var processInfo = await _processInfoRepository.GetAsync(_processInfoRepository.ActiveInstance);
                var fileName = Guid.NewGuid().ToString();

                zipFilePath = await _dwcArchiveFileWriter.CreateDwcArchiveFileAsync(
                    filter,
                    fileName,
                    _processedSightingRepository,
                    FieldDescriptionHelper.GetDefaultDwcExportFieldDescriptions(),
                    processInfo,
                    _exportPath,
                    cancellationToken);
                cancellationToken?.ThrowIfCancellationRequested();

                // Make sure container exists
                var container = $"sos-{DateTime.Now.Year}";
                await _blobStorageService.CreateContainerAsync(container);

                // Upload file to blob storage
                if (await _blobStorageService.UploadBlobAsync(zipFilePath, container))
                {
                    // Remove local file
                    _fileService.DeleteFile(zipFilePath);

                    return $"{fileName}.zip";
                }

                return null;
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
            var fileName = await ExportDWCAsync(new AdvancedFilter(), cancellationToken);
           
            return !string.IsNullOrEmpty(fileName);
        }
    }
}