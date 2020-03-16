using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Jobs.Process;
using SOS.Process.Factories.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Artportalen harvest
    /// </summary>
    public class ActivateInstanceJob : IActivateInstanceJob
    {
        private readonly IInstanceFactory _instanceFactory;
        private readonly ILogger<ActivateInstanceJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instanceFactory"></param>
        /// <param name="logger"></param>
        public ActivateInstanceJob(
            IInstanceFactory instanceFactory,
            ILogger<ActivateInstanceJob> logger)
        {
            _instanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(byte instance)
        {
            try
            {
                // Activate passed instance
                var success = await _instanceFactory.SetActiveInstanceAsync(instance);
                return success ? true : throw new Exception("Activate instance job failed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to active instance");
                return false;
            }
        }
    }
}
