using Hangfire;
using SOS.Harvest.Jobs.Interfaces;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Harvest.Jobs
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

        protected IEnumerable<string> GetOnGoingJobIds(params string[] filter)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            return monitoringApi.ProcessingJobs(0, (int)monitoringApi.ProcessingCount())?
                .Where(j => j.Value.InProcessingState && filter.Contains(j.Value.Job.Type.Name, StringComparer.CurrentCultureIgnoreCase))
                .Select(j => j.Key)?.ToArray()!;
        }
         
        protected void RestartJobs(IEnumerable<string> jobIds)
        {
            if (!jobIds?.Any() ?? true)
            {
                return;
            }

            foreach (var jobId in jobIds)
            {
                BackgroundJob.Delete(jobId);
                Thread.Sleep(30000); // Sleep 30s to let job finish
                BackgroundJob.Requeue(jobId);
            }
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
    }
}