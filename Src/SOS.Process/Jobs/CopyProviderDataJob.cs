using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Process.Factories.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class CopyProviderDataJob : ICopyProviderDataJob
    {
        private readonly IInstanceFactory _instanceFactory;
        private readonly ILogger<CopyProviderDataJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instanceFactory"></param>
        /// <param name="logger"></param>
        public CopyProviderDataJob(
            IInstanceFactory instanceFactory,
            ILogger<CopyProviderDataJob> logger)
        {
            _instanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(DataProvider provider)
        {
            try
            {
                // Activate passed instance
                var success =  await _instanceFactory.CopyProviderDataAsync(provider);
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
