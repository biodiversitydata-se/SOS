using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Managers.Interfaces
{
    public interface IDataProviderManager
    {
        Task<bool> AddDataProvider(DataProvider dataProvider);
        Task<bool> DeleteDataProvider(int id);
        Task<bool> UpdateDataProvider(int id, DataProvider dataProvider);
        Task<bool> InitDefaultDataProviders();
        Task<DataProvider> TryGetDataProviderAsync(int id);
    }
}
