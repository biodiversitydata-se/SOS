using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Processed data class
    /// </summary>
    public interface IProcessInfoRepository : IMongoDbProcessedRepositoryBase<ProcessInfo, string>
    {
        Task<bool> CopyProviderDataAsync(DataProvider dataProvider);
    }
}