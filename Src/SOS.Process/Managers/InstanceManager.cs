using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Process.Managers.Interfaces;

namespace SOS.Process.Managers
{
    /// <summary>
    ///     Instance manager class
    /// </summary>
    public class InstanceManager : ManagerBase<InstanceManager>, IInstanceManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="logger"></param>
        public InstanceManager(
            IProcessedObservationRepository processedObservationRepository,            
            ILogger<InstanceManager> logger) : base(processedObservationRepository, logger)
        {
            
        }     

        public async Task<bool> SetActiveInstanceAsync(byte instance)
        {
            try
            {
                Logger.LogDebug($"Activating instance: {instance}");

                return await ProcessedObservationRepository.SetActiveInstanceAsync(instance);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to activate instance: {instance}");
                return false;
            }
        }
    }
}