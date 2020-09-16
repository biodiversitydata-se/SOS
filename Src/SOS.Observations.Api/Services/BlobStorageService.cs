using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Misc;
using SOS.Observations.Services.Interfaces;

namespace SOS.Observations.Services
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
        public string GetDOIDownloadUrl(Guid id)
        {
            var fileName = $"{id}.zip";

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
    }
}