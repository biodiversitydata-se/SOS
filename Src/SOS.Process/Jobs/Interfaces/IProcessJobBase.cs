using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Jobs.Interfaces
{
    public interface IProcessJobBase
    {
        /// <summary>
        ///     Get a harvest info item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<HarvestInfo> GetHarvestInfoAsync(string id);

        /// <summary>
        ///     Get a process info item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProcessInfo> GetProcessInfoAsync(string id);

        /// <summary>
        ///     Save process info
        /// </summary>
        /// <param name="processInfoId"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="status"></param>
        /// <param name="providersInfo"></param>
        /// <returns></returns>
        Task SaveProcessInfo(string processInfoId, DateTime start, int count, RunStatus status,
            IEnumerable<ProviderInfo> providersInfo);
    }
}