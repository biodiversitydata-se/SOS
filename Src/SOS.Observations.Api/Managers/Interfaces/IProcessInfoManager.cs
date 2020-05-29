using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Process information manager
    /// </summary>
    public interface IProcessInfoManager
    {
        /// <summary>
        ///     Get process information
        /// </summary>
        /// <returns></returns>
        Task<ProcessInfo> GetProcessInfoAsync(bool active);
    }
}