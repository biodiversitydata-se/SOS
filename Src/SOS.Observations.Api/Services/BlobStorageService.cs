using System;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Observations.Services.Interfaces;

namespace SOS.Observations.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly string _doiContainer;
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
            _doiContainer = blobStorageConfiguration.DOI_Container;
        }

        /// <inheritdoc />
        public string GetDOIDownloadUrl(Guid id)
        {
            var cloudBlobContainer = _cloudBlobClient.GetContainerReference(_doiContainer);
            var fileName = $"{id}.zip";
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
    }
}