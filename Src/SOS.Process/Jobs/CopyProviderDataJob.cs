using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Process.Managers.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Artportalen harvest
    /// </summary>
    public class CopyProviderDataJob : ICopyProviderDataJob
    {
        private readonly IInstanceManager _instanceManager;
        private readonly ILogger<CopyProviderDataJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instanceManager"></param>
        /// <param name="logger"></param>
        public CopyProviderDataJob(
            IInstanceManager instanceManager,
            ILogger<CopyProviderDataJob> logger)
        {
            _instanceManager = instanceManager ?? throw new ArgumentNullException(nameof(instanceManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(DataProvider provider)
        {
            try
            {
                // Activate passed instance
                var success =  await _instanceManager.CopyProviderDataAsync(provider);
                return success ? true : throw new Exception("Copy provider data job failed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to copy provider data");
                return false;
            }
        }
    }
}
