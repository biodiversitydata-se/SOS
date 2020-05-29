using System.Threading.Tasks;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Resource.Interfaces
{
    public interface IDataProviderRepository : IResourceRepositoryBase<DataProvider, int>
    {
        Task<bool> UpdateHarvestInfo(int dataProviderId, HarvestInfo harvestInfo);
    }
}