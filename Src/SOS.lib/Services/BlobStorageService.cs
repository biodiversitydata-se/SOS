using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Services.Interfaces;
using File = SOS.Lib.Models.Misc.File;

namespace SOS.Lib.Services
{
    /// <summary>
    /// Service for blob storage
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _doiContainer;
        private readonly string _exportContainer;
        private readonly ILogger<BlobStorageService> _logger;

        private BlobContainerClient GetContainerClient(string container) =>
            _blobServiceClient.GetBlobContainerClient(container);

        private BlobClient GetBlobClient(string container, string fileName) =>
            GetContainerClient(container)?.GetBlobClient(fileName);

        private string GetFileDownloadUrl(string container, string fileName)
        {
            var blobClient = GetBlobClient(container, fileName);

            return (blobClient.CanGenerateSasUri
                ? blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.Now)
                : blobClient?.Uri)?.ToString();
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
            _blobServiceClient = new BlobServiceClient(blobStorageConfiguration.ConnectionString);
            _doiContainer = blobStorageConfiguration.Containers["doi"];
            _exportContainer = blobStorageConfiguration?.Containers["export"];
        }

        /// <inheritdoc />
        public async Task<bool> CopyFileAsync(string sourceContainer, string sourceFileName, string targetContainer, string targetFileName)
        {
            try
            {
                var sourceBlobClient = GetBlobClient(sourceContainer, sourceFileName);

                if (!await sourceBlobClient.ExistsAsync())
                {
                    return false;
                }

                var targetBlobClient = GetBlobClient(sourceContainer, sourceFileName);

                await  targetBlobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                await using var sourceStream = sourceBlobClient.OpenRead(new BlobOpenReadOptions(false));

                await targetBlobClient.UploadAsync(sourceStream);

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
                var containerClient = GetContainerClient(name);

                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create container");

                return false;
            }
        }

        /// <inheritdoc />
        public string GetDOIDownloadUrl(string prefix, string suffix)
        {
            var fileName = $"{prefix}/{suffix}.zip";

            return GetFileDownloadUrl(_doiContainer, fileName);
        }

        /// <inheritdoc />
        public string GetExportDownloadUrl(string fileName)
        {
            return GetFileDownloadUrl(_exportContainer, fileName);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<File>> GetExportFilesAsync()
        {
            var containerClient = GetContainerClient(_exportContainer);

            if (!await containerClient.ExistsAsync())
            {
                _logger.LogDebug($"Container {_exportContainer} doesn't exists");
                return null;
            }

            // Call the listing operation and return pages of the specified size.
            var blobs = containerClient.GetBlobsAsync();

            var files = new List<File>();

            // Enumerate the blobs returned for each page.
            await foreach (var blob in blobs)
            {
                files.Add(new File
                {
                    Created = Convert.ToDateTime(blob.Properties.LastModified.ToString()),
                    Name = blob.Name,
                    Size = blob.Properties.ContentLength ?? 0,
                    DownloadUrl = GetExportDownloadUrl(blob.Name)
                });
            }

            return files;
        }

        /// <inheritdoc />
        public async Task<bool> UploadBlobAsync(string sourcePath, string container)
        {
            try
            {
                var blobName =
                    sourcePath.Substring(sourcePath.LastIndexOf(@"\", StringComparison.CurrentCultureIgnoreCase) + 1);

                var blobClient = GetBlobClient(container, blobName);
                await using var blob = System.IO.File.OpenRead(sourcePath);
                await blobClient.UploadAsync(blob, true);
                blob.Close();

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