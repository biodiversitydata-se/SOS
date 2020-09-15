using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Jobs.Export;

namespace SOS.Export.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class UploadToStoreJob : IUploadToStoreJob
    {
        private readonly ILogger<UploadToStoreJob> _logger;
        private readonly IBlobStorageService _blobStorageService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="blobStorageService"></param>
        /// <param name="logger"></param>
        public UploadToStoreJob(IBlobStorageService blobStorageService, ILogger<UploadToStoreJob> logger)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(string sourcePath, string blobStorageContainer,
            bool deleteSourceOnSuccess, IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start upload to store job");

                // Blob Storage Containers must be in lower case
                blobStorageContainer = blobStorageContainer?.ToLower();

                // Make sure container exists
                await _blobStorageService.CreateContainerAsync(blobStorageContainer);

                cancellationToken?.ThrowIfCancellationRequested();

                // Upload file to blob storage
                var success = await _blobStorageService.UploadBlobAsync(sourcePath, blobStorageContainer);

                _logger.LogInformation($"End upload to store job. Success: {success}");

                if (success && deleteSourceOnSuccess)
                {
                    _logger.LogInformation("Delete source file.");
                    File.Delete(sourcePath);
                }

                return success ? true : throw new Exception("Upload to store job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Upload to store job was cancelled.");
                return false;
            }
        }
    }
}