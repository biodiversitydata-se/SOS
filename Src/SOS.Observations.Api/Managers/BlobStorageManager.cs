﻿using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Misc;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Managers.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOS.Observations.Api.Managers
{
    /// <summary>
    ///     Area manager
    /// </summary>
    public class BlobStorageManager : IBlobStorageManager
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<BlobStorageManager> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="blobStorageService"></param>
        /// <param name="logger"></param>
        public BlobStorageManager(
            IBlobStorageService blobStorageService,
            ILogger<BlobStorageManager> logger)
        {
            _blobStorageService = blobStorageService ??
                                  throw new ArgumentNullException(nameof(blobStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string GetExportDownloadUrl(string fileName)
        {
            return _blobStorageService.GetExportDownloadUrl(fileName);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<File>> GetExportFilesAsync()
        {
            return await _blobStorageService.GetExportFilesAsync();
        }
    }
}