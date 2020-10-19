using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Managers.Interfaces
{
    public interface IDataProviderManager
    {
        Task<List<DataProvider>> GetAllDataProvidersAsync();
        Task<DataProvider> GetDataProviderByIdAsync(int id);
    }
}