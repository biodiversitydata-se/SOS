using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Shared;
using SOS.Process.Managers.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Managers
{
    /// <summary>
    ///     Instance manager class
    /// </summary>
    public class InstanceManager : ManagerBase<InstanceManager>, IInstanceManager
    {
        private readonly IProcessInfoRepository _processInfoRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processedObservationRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="logger"></param>
        public InstanceManager(
            IProcessedObservationRepository processedObservationRepository,
            IProcessInfoRepository processInfoRepository,
            ILogger<InstanceManager> logger) : base(processedObservationRepository, logger)
        {
            _processInfoRepository =
                processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
        }

        /// <inheritdoc />
        public async Task<bool> CopyProviderDataAsync(DataProvider dataProvider)
        {
            try
            {
                Logger.LogDebug("Start deleting data from inactive instance");
                if (!await ProcessRepository.DeleteProviderDataAsync(dataProvider))
                {
                    Logger.LogError("Failed to delete from inactive instance");
                    return false;
                }

                Logger.LogDebug("Finish deleting data from inactive instance");

                Logger.LogDebug("Start copying data from active to inactive instance");
                if (await ProcessRepository.CopyProviderDataAsync(dataProvider))
                {
                    Logger.LogDebug("Finish copying data from active to inactive instance");

                    Logger.LogDebug("Start copying metadata from active to inactive instance");
                    var res = await _processInfoRepository.CopyProviderDataAsync(dataProvider);
                    Logger.LogDebug("Finish copying metadata from active to inactive instance");

                    return res;
                }

                return false;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to copy from active to inactive instance");
                return false;
            }
        }

        public async Task<bool> SetActiveInstanceAsync(byte instance)
        {
            try
            {
                Logger.LogDebug($"Activating instance: {instance}");

                return await ProcessRepository.SetActiveInstanceAsync(instance);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to activate instance: {instance}");
                return false;
            }
        }
    }
}