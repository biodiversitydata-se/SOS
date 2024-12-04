﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Harvest.Managers
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
            IProcessedObservationCoreRepository processedObservationRepository,
            ILogger<InstanceManager> logger) : base(processedObservationRepository, logger)
        {

        }

        public async Task<bool> SetActiveInstanceAsync(byte instance)
        {
            try
            {
                Logger.LogDebug("Activating instance: {@activeInstance}", instance);

                return await ProcessedObservationRepository.SetActiveInstanceAsync(instance);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to activate instance: {@activeInstance}", instance);
                return false;
            }
        }
    }
}