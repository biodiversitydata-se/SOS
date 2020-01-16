using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Factories
{
    /// <summary>
    /// Process factory class
    /// </summary>
    public class InstanceFactory : ProcessBaseFactory<InstanceFactory>, Interfaces.IInstanceFactory
    {
        private readonly IProcessInfoRepository _processInfoRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedSightingRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="logger"></param>
        public InstanceFactory(
            IProcessedSightingRepository processedSightingRepository,
            IProcessInfoRepository processInfoRepository,
            ILogger<InstanceFactory> logger) : base(processedSightingRepository, logger)
        {
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
        }

        /// <inheritdoc />
        public async Task<bool> CopyProviderDataAsync(DataProvider provider)
        {
            try
            {
                Logger.LogDebug("Start deleting data from inactive instance");
                if (!await ProcessRepository.DeleteProviderDataAsync(provider))
                {
                    Logger.LogError("Failed to delete clam portal data");
                    return false;
                }

                Logger.LogDebug("Start copying data from active to inactive instance");
                if (await ProcessRepository.CopyProviderDataAsync(provider))
                {
                    Logger.LogDebug("Start copying metadata from active to inactive instance");
                    return await _processInfoRepository.CopyProviderDataAsync(provider);
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.LogError(e,"Failed to copy from active to inactive instance");
                return false;
            }
        }

        public async Task<bool> SetActiveInstanceAsync(byte instance)
        {
            try
            {
                Logger.LogDebug($"Activating instance: { instance }");

                return await ProcessRepository.SetActiveInstanceAsync(instance);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to activate instance: { instance }");
                return false;
            }
        }
    }
}
