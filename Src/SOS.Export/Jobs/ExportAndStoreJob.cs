﻿using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Threading.Tasks;

namespace SOS.Export.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ExportAndStoreJob : IExportAndStoreJob
    {
        private readonly ILogger<ExportAndStoreJob> _logger;
        private readonly IObservationManager _observationManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="logger"></param>
        public ExportAndStoreJob(IObservationManager observationManager, ILogger<ExportAndStoreJob> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(SearchFilter filter, string blobStorageContainer, string fileName,
            IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start export and store job");
                var success = await _observationManager.ExportAndStoreAsync(filter, blobStorageContainer, fileName, "DwC-A file",
                    cancellationToken);

                _logger.LogInformation($"End export and store job. Success: {success}");

                return success ? true : throw new Exception("Export and store job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export and store job was cancelled.");
                return false;
            }
        }
    }
}