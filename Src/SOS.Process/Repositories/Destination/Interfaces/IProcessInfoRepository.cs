
using System;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Processed data class
    /// </summary>
    public interface IProcessInfoRepository : IProcessBaseRepository<ProcessInfo, string>
    {
        Task<bool> CopyProviderDataAsync(DataProvider dataProvider);
    }
}
