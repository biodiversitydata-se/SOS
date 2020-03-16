using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;

namespace SOS.Observations.Api.Factories.Interfaces
{
    /// <summary>
    /// Process information factory
    /// </summary>
    public interface IProcessInfoFactory
    {
        /// <summary>
        /// Get process information 
        /// </summary>
        /// <returns></returns>
        Task<ProcessInfo> GetProcessInfoAsync(bool active);
    }
}
