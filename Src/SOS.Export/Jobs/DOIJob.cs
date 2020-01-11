using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Factories.Interfaces;
using SOS.Export.Jobs.Interfaces;
using SOS.Lib.Models.Search;

namespace SOS.Export.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class DOIJob : IDOIJob
    {
        private readonly ISightingFactory _sightingFactory;
        private readonly ILogger<DOIJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sightingFactory"></param>
        /// <param name="logger"></param>
        public DOIJob(ISightingFactory sightingFactory, ILogger<DOIJob> logger)
        {
            _sightingFactory = sightingFactory ?? throw new ArgumentNullException(nameof(sightingFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(AdvancedFilter filter, string fileName, IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Start DOI job");
                var success = await _sightingFactory.CreateDOIAsync(filter, fileName, cancellationToken);
                _logger.LogDebug($"End DOI job. Success: {success}");
                return success;
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("DOI job was cancelled.");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "DOI job failed");
                return false;
            }
        }
    }
}
