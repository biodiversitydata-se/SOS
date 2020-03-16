using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Factories
{
    /// <summary>
    /// Sighting factory class
    /// </summary>
    public class ProcessInfoFactory : Interfaces.IProcessInfoFactory
    {
        private readonly IProcessInfoRepository _processInfoRepository;

        private readonly ILogger<ProcessInfoFactory> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processInfoRepository"></param>
        /// <param name="logger"></param>
        public ProcessInfoFactory(
            IProcessInfoRepository processInfoRepository,
            ILogger<ProcessInfoFactory> logger)
        {
            _processInfoRepository = processInfoRepository ??
                                           throw new ArgumentNullException(nameof(processInfoRepository));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<ProcessInfo> GetProcessInfoAsync(bool active)
        {
            try
            {
                var processInfo = await _processInfoRepository.GetProcessInfoAsync(active);
                return processInfo;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get current process info");
                return null;
            }
        }
    }
}
