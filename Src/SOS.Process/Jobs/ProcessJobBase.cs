using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Jobs.Interfaces;

namespace SOS.Process.Jobs
{
    public class ProcessJobBase : IProcessJobBase
    {
        private readonly IHarvestInfoRepository _harvestInfoRepository;
        private readonly IProcessInfoRepository _processInfoRepository;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="processInfoRepository"></param>
        protected ProcessJobBase(
            IHarvestInfoRepository harvestInfoRepository,
            IProcessInfoRepository processInfoRepository)
        {
            _harvestInfoRepository =
                harvestInfoRepository ?? throw new ArgumentNullException(nameof(harvestInfoRepository));
            _processInfoRepository =
                processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
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


        protected async Task SaveProcessInfo(ProcessInfo processInfo)
        {
            // Make sure collection exists
            await _processInfoRepository.VerifyCollectionAsync();

            await _processInfoRepository.AddOrUpdateAsync(processInfo);
        }


        /// <summary>
        ///     Get provider info
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<ProcessInfo>> GetProcessInfoAsync(IEnumerable<string> ids)
        {
            var processInfos = new List<ProcessInfo>();
            foreach (var id in ids)
            {
                var processInfo = await GetProcessInfoAsync(id);

                if (processInfo != null)
                {
                    processInfos.Add(processInfo);
                }
            }

            return processInfos;
        }

        protected async Task<ProcessInfo> GetObservationProcessInfoAsync(bool active)
        {
            return await _processInfoRepository.GetProcessInfoAsync(active);
        }
    }
}