using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using SOS.Export.Factories.Interfaces;
using SOS.Export.Jobs.Interfaces;

namespace SOS.Export.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class ExportDarwinCoreJob : IExportDarwinCoreJob
    {
        private readonly ISightingFactory _sightingFactory;
        private readonly ILogger<ExportDarwinCoreJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sightingFactory"></param>
        /// <param name="logger"></param>
        public ExportDarwinCoreJob(ISightingFactory sightingFactory, ILogger<ExportDarwinCoreJob> logger)
        {
            _sightingFactory = sightingFactory ?? throw new ArgumentNullException(nameof(sightingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> Run(IJobCancellationToken cancellationToken)
        {
            _logger.LogDebug("Start Export Darwin Core job");
           
            var success = await _sightingFactory.ExportAllAsync(cancellationToken);

            _logger.LogDebug($"End Export Darwin Core job. Success: {success}");

            // return result of all imports
            return success;
        }
    }
}
