
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IProcessInfoRepository : IProcessBaseRepository<ProcessInfo, byte>
    {
        Task<bool> CopyProviderDataAsync(DataProvider provider);
    }
}
