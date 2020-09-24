using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Misc;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly string _doiContainer;
        private readonly string _exportContainer;
        private readonly ILogger<BlobStorageService> _logger;

        private string GetFileDownloadUrl(string container, string fileName)
        {
            var cloudBlobContainer = _cloudBlobClient.GetContainerReference(container);
            var blockBlobReference = cloudBlobContainer.GetBlockBlobReference(fileName);

            //Create an ad-hoc Shared Access Policy with read permissions which will expire in 12 hours
            var policy = new SharedAccessBlobPolicy
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(12)
            };
            //Set content-disposition header for force download
            var headers = new SharedAccessBlobHeaders
            {
                ContentDisposition = $"attachment;filename=\"{fileName}\""
            };
            var sasToken = blockBlobReference.GetSharedAccessSignature(policy, headers);

            return blockBlobReference.Uri.AbsoluteUri + sasToken;
        }

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

            _logger.LogDebug($"Tries to connect to blob storage: {blobStorageConfiguration.ConnectionString?.Substring(0, 10)}...");
            if (!CloudStorageAccount.TryParse(blobStorageConfiguration.ConnectionString, out var storageAccount))
            {
                _logger.LogError($"Failed to connect to blob storage ({blobStorageConfiguration.ConnectionString})");
                throw new Exception("Failed to connect to blob storage");
            }

            _cloudBlobClient = storageAccount.CreateCloudBlobClient();
            _doiContainer = blobStorageConfiguration?.Containers["doi"];
            _exportContainer = blobStorageConfiguration?.Containers["export"];
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
        public string GetDOIDownloadUrl(string suffix)
        {
            var fileName = $"{suffix}.zip";

            return GetFileDownloadUrl(_doiContainer, fileName);
        }

        /// <inheritdoc />
        public string GetExportDownloadUrl(string fileName)
        {
            return GetFileDownloadUrl(_exportContainer, fileName);
        }

        /// <inheritdoc />
        public IEnumerable<File> GetExportFiles()
        {
            var cloudBlobContainer = _cloudBlobClient.GetContainerReference(_exportContainer);

            if (!cloudBlobContainer.Exists())
            {
                _logger.LogDebug($"Container {_exportContainer} doesn't exists");
                return null;
            }

            var blobs = cloudBlobContainer.ListBlobs()?.OfType<CloudBlockBlob>()?.ToList();
            return
                from b
                    in blobs
                select new File()
                {
                    Created = Convert.ToDateTime(b.Properties.LastModified.ToString()),
                    Name = b.Name,
                    Size = b.Properties.Length
                };
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