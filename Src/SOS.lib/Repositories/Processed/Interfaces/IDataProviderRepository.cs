using SOS.Lib.Models.Shared;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    public interface IDataProviderRepository : IMongoDbProcessedRepositoryBase<DataProvider, int>
    {
    }
}