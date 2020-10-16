using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessInfoRepository : MongoDbProcessedRepositoryBase<ProcessInfo, string>, IProcessInfoRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessInfoRepository(
            IProcessClient client,
            ILogger<ProcessInfoRepository> logger
        ) : base(client, false, logger)
        {
        }


        /// <inheritdoc />
        public async Task<bool> CopyProviderDataAsync(DataProvider dataProvider)
        {
            var activeProcessedInfoId = $"{nameof(Observation)}-{ActiveInstance}";
            var inactiveProcessedInfoId = $"{nameof(Observation)}-{InActiveInstance}";
            // Get data from active instance
            var source = await GetAsync(activeProcessedInfoId);
            var sourceProvider =
                source?.ProvidersInfo?.FirstOrDefault(pi => pi.DataProviderIdentifier.Equals(dataProvider.Identifier));

            if (sourceProvider == null)
            {
                return false;
            }

            var target = await GetAsync(inactiveProcessedInfoId) ??
                         new ProcessInfo(InactiveInstanceName, DateTime.Now);

            // make a list of providers
            var targetProviders = target.ProvidersInfo.ToList();

            // Remove provider data
            targetProviders.RemoveAll(p => p.DataProviderIdentifier.Equals(dataProvider.Identifier));

            // Add provider data from active instance and update db document
            targetProviders.Add(sourceProvider);
            target.ProvidersInfo = targetProviders;
            target.Count = target.ProvidersInfo.Sum(pi => pi.ProcessCount ?? 0);
            return await AddOrUpdateAsync(target);
        }
    }
}