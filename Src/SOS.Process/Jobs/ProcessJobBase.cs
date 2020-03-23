using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Jobs.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Jobs
{
    public class ProcessJobBase : IProcessJobBase
    {
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IProcessInfoRepository _processInfoRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="processInfoRepository"></param>
        protected ProcessJobBase(IHarvestInfoRepository harvestInfoRepository,
            IProcessInfoRepository processInfoRepository)
        {
            _harvestInfoRepository = harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
        }

        /// <summary>
        /// Create a provider info object
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="harvestInfo"></param>
        /// <param name="processStart"></param>
        /// <param name="processEnd"></param>
        /// <param name="processStatus"></param>
        /// <param name="processCount"></param>
        /// <returns></returns>
        protected ProviderInfo CreateProviderInfo(DataSet provider, HarvestInfo harvestInfo, DateTime processStart, DateTime? processEnd = null, RunStatus? processStatus = null, int? processCount = null )
        {
            return new ProviderInfo(provider)
            {
                HarvestCount = harvestInfo?.Count,
                HarvestEnd = harvestInfo?.End,
                HarvestStart = harvestInfo?.Start,
                HarvestStatus = harvestInfo?.Status,
                ProcessCount = processCount,
                ProcessEnd = processEnd,
                ProcessStart = processStart,
                ProcessStatus = processStatus
            };
        }

        /// <inheritdoc />
        public async Task<HarvestInfo> GetHarvestInfoAsync(string id)
        {
            return await _harvestInfoRepository.GetAsync(id);
        }

        /// <inheritdoc />
        public async Task<ProcessInfo> GetProcessInfoAsync(string id)
        {
            return await _processInfoRepository.GetAsync(id);
        }

        /// <summary>
        /// Get provider info
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ProviderInfo>> GetProviderInfoAsync(IDictionary<string, DataSet> ids)
        {
            var providerInfo = new List<ProviderInfo>();
            foreach (var id in ids)
            {
                var processInfo = await GetProcessInfoAsync(id.Key);

                if (processInfo != null)
                {
                    providerInfo.Add(processInfo.ProvidersInfo?.FirstOrDefault(p=> p.Provider == id.Value));
                }
            }

            return providerInfo;
        }

        /// <inheritdoc />
        public async Task SaveProcessInfo(string processInfoId, DateTime start, int count, RunStatus status, IEnumerable<ProviderInfo> providersInfo)
        {
            // Make sure collection exists
            await _processInfoRepository.VerifyCollectionAsync();

            var processInfo = new ProcessInfo(processInfoId, start)
            {
                Count = count,
                End = DateTime.Now,
                ProvidersInfo = providersInfo,
                Status = status
            };

            await _processInfoRepository.AddOrUpdateAsync(processInfo);
        }
    }
}
