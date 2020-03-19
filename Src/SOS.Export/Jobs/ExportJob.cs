using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.Search;

namespace SOS.Export.Jobs
{
    /// <summary>
    /// Artportalen harvest
    /// </summary>
    public class ExportJob : IExportJob
    {
        private readonly IObservationManager _observationManager;
        private readonly ILogger<ExportJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="logger"></param>
        public ExportJob(IObservationManager observationManager, ILogger<ExportJob> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(ExportFilter filter, string email, IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Start export job");
                var success = await _observationManager.ExportDWCAsync(filter, email, cancellationToken);

                _logger.LogDebug($"End DOI job. Success: {success}");
                
                return success ? true : throw new Exception("Export Job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Export job failed");
                throw new Exception("Export Job failed");
            }
        }
    }
}
