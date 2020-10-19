using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Models.Shared;

namespace SOS.Process.Managers.Interfaces
{
    public interface IDataProviderManager
    {
        Task<bool> AddDataProvider(DataProvider dataProvider);
        Task<bool> DeleteDataProvider(int id);
        Task<bool> UpdateDataProvider(int id, DataProvider dataProvider);
        Task<bool> InitDefaultDataProviders();
        Task<List<DataProvider>> GetAllDataProvidersAsync();
        Task<DataProvider> GetDataProviderByIdAsync(int id);
        Task<DataProvider> GetDataProviderByIdOrIdentifier(string dataProviderIdOrIdentifier);
        Task<DataProvider> GetDataProviderByIdentifier(string identifier);
        Task<List<Result<DataProvider>>> GetDataProvidersByIdOrIdentifier(List<string> dataProviderIdOrIdentifiers);
    }
}