using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;

namespace SOS.Search.Service.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessInfoRepository : IBaseRepository<ProcessInfo, byte>
    {
        /// <summary>
        /// Get process information of active instance
        /// </summary>
        /// <returns></returns>
        Task<ProcessInfo> GetCurrentProcessInfoAsync();
    }
}
