using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using SOS.Export.Services.Interfaces;
using SOS.Lib.Configuration.Shared;

namespace SOS.Export.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly ILogger<BlobStorageService> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="blobStorageConfiguration"></param>
        /// <param name="logger"></param>
        public BlobStorageService(BlobStorageConfiguration blobStorageConfiguration,
            ILogger<BlobStorageService> logger)
        {
            if (blobStorageConfiguration == null)
            {
                throw new ArgumentNullException(nameof(blobStorageConfiguration));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!CloudStorageAccount.TryParse(blobStorageConfiguration.ConnectionString, out var storageAccount))
            {
                _logger.LogError("Failed to connect to blob storage");
                throw new Exception("Failed to connect to blob storage");
            }

            _cloudBlobClient = storageAccount.CreateCloudBlobClient();
        }

        /// <inheritdoc />
        public async Task<bool> CopyFileAsync(string sourceContainer, string sourceFileName, string targetContainer, string targetFileName)
        {
            try
            {
                var source = _cloudBlobClient.GetContainerReference(sourceContainer);
                var sourceBlob = source.GetBlockBlobReference(sourceFileName);
                var target = _cloudBlobClient.GetContainerReference(targetContainer);
                var targetBlob = target.GetBlobReference(targetFileName);
                
                await targetBlob.StartCopyAsync(sourceBlob.Uri);

                while (targetBlob.CopyState.Status == CopyStatus.Pending)
                {
                    Thread.Sleep(500);
                    await targetBlob.FetchAttributesAsync();
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to copy file");

                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> CreateContainerAsync(string name)
        {
            try
            {
                var cloudBlobContainer = _cloudBlobClient.GetContainerReference(name);

                if (await cloudBlobContainer.CreateIfNotExistsAsync())
                {
                    var permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudBlobContainer.SetPermissionsAsync(permissions);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create container");

                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UploadBlobAsync(string sourcePath, string container)
        {
            try
            {
                var cloudBlobContainer = _cloudBlobClient.GetContainerReference(container);
                var blobName =
                    sourcePath.Substring(sourcePath.LastIndexOf(@"\", StringComparison.CurrentCultureIgnoreCase) + 1);
                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                await cloudBlockBlob.UploadFromFileAsync(sourcePath);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to upload blob");

                return false;
            }
        }
    }
}