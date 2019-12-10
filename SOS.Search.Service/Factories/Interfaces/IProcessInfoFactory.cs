using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;

namespace SOS.Search.Service.Factories.Interfaces
{
    /// <summary>
    /// Process information factory
    /// </summary>
    public interface IProcessInfoFactory
    {
        /// <summary>
        /// Get process information of active instance
        /// </summary>
        /// <returns></returns>
        Task<ProcessInfo> GetCurrentProcessInfoAsync();
    }
}
