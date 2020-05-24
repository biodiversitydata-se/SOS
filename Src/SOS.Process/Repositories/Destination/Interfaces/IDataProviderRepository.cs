using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    public interface IDataProviderRepository : IProcessBaseRepository<DataProvider, int>
    {
        Task<bool> UpdateProcessInfo(int dataProviderId, string collectionName, ProviderInfo providerInfo);
    }
}
