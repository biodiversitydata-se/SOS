using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Process.Factories.Interfaces;
using SOS.Process.Jobs.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class CopyProviderDataJob : ICopyProviderDataJob
    {
        private readonly IInstanceFactory _instanceFactory;
        private readonly ILogger<ActivateInstanceJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instanceFactory"></param>
        /// <param name="logger"></param>
        public CopyProviderDataJob(
            IInstanceFactory instanceFactory,
            ILogger<ActivateInstanceJob> logger)
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
                return await _instanceFactory.CopyProviderDataAsync(provider);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to copy provider data");
                return false;
            }
        }
    }
}
