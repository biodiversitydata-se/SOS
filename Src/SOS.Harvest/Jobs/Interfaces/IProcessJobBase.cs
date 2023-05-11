using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Harvest.Jobs.Interfaces
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
    }
}