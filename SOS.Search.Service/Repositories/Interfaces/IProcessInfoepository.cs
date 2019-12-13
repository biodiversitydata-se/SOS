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
        /// Get process information
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        Task<ProcessInfo> GetProcessInfoAsync(bool current);
    }
}
