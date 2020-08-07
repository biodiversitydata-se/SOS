using System.Threading.Tasks;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    public interface IDataProviderRepository : IMongoDbProcessedRepositoryBase<DataProvider, int>
    {
        Task<bool> UpdateProcessInfo(int dataProviderId, string collectionName, ProviderInfo providerInfo);
    }
}