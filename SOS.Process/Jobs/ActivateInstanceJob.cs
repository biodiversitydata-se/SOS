using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Process.Jobs.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    /// Species portal harvest
    /// </summary>
    public class ActivateInstanceJob : IActivateInstanceJob
    {
        private readonly IDarwinCoreRepository _processRepository;
        private readonly ILogger<ActivateInstanceJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processRepository"></param>
        /// <param name="logger"></param>
        public ActivateInstanceJob(
            IDarwinCoreRepository processRepository,
            ILogger<ActivateInstanceJob> logger)
        {
            _processRepository = processRepository ?? throw new ArgumentNullException(nameof(processRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RunAsync(byte instance)
        {
            try
            {
                // Activate passed instance
                return await _processRepository.SetActiveInstanceAsync(instance);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to active instance");
                return false;
            }
        }
    }
}
