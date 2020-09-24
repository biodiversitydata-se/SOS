using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ExportToDoiJob : IExportToDoiJob
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly string _doiContainer;
        private readonly string _exportContainer;
        private readonly ILogger<ExportToDoiJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="blobStorageService"></param>
        /// <param name="logger"></param>
        public ExportToDoiJob(IBlobStorageService blobStorageService, BlobStorageConfiguration configuration, ILogger<ExportToDoiJob> logger)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _doiContainer = configuration?.Containers["doi"] ?? throw new ArgumentNullException(nameof(configuration));
            _exportContainer = configuration.Containers["export"];

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(string fileName, IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start export to DOI job");

                _logger.LogDebug($"Start copy file ({fileName}) from {_exportContainer} to {_doiContainer}");

                // Todo get DOI information from DataCite

                var success = await _blobStorageService.CopyFileAsync(_exportContainer, fileName, _doiContainer, $"10.####/{Guid.NewGuid()}.zip");
                _logger.LogDebug($"Finish copy file ({fileName}) from {_exportContainer} to {_doiContainer}");

                _logger.LogInformation($"End export to DOI job. Success: {success}");

                return success ? true : throw new Exception("Export to DOI job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export to DOI job was cancelled.");
                return false;
            }
        }
    }
}