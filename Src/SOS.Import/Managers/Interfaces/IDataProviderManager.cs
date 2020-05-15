using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Managers.Interfaces
{
    public interface IDataProviderManager
    {
        Task<bool> AddDataProvider(DataProvider dataProvider);
        Task<bool> DeleteDataProvider(int id);
        Task<bool> UpdateDataProvider(int id, DataProvider dataProvider);

        /// <summary>
        /// Initialize DataProvider collection with default data providers.
        /// </summary>
        /// <param name="forceOverwriteIfCollectionExist">If the DataProvider collection already exists, set forceOverwriteIfCollectionExist to true if you want to overwrite this collection with default data.</param>
        /// <returns></returns>
        Task<Result<string>> InitDefaultDataProviders(bool forceOverwriteIfCollectionExist);
        Task<DataProvider> TryGetDataProviderAsync(int id);
    }
}
