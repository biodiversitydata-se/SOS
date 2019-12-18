using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class ProcessInfoRepository : ProcessBaseRepository<ProcessInfo, byte>, IProcessInfoRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessInfoRepository(
            IProcessClient client,
            ILogger<ProcessInfoRepository> logger
        ):base(client, false, logger)
        {

        }

        /// <inheritdoc />
        public async Task<bool> CopyProviderDataAsync(DataProvider provider)
        {
            // Get data from active instance
            var source = await GetAsync(ActiveInstance);

            var sourceProvider = source?.ProviderInfo?.FirstOrDefault(pi => pi.Provider.Equals(provider));

            if (sourceProvider == null)
            {
                return false;
            }

            var target = (await GetAsync(InstanceToUpdate)) ?? new ProcessInfo(InstanceToUpdate);

            // make a list of providers
            var targetProviders = target.ProviderInfo.ToList();

            // Remove provider data
            targetProviders.RemoveAll(p => p.Provider.Equals(provider));

            // Add provider data from active instance and update db document
            targetProviders.Add(sourceProvider);
            target.ProviderInfo = targetProviders;

            return await AddOrUpdateAsync(target);
        }
    }
}
