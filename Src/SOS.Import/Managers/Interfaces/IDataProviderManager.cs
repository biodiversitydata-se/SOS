using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
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

        Task<List<DataProvider>> GetAllDataProviders();

        /// <summary>
        /// Get data provider by Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<DataProvider> GetDataProviderByIdAsync(int id);

        /// <summary>
        /// Get data provider by Id or Identifier.
        /// </summary>
        /// <param name="dataProviderIdOrIdentifier"></param>
        /// <returns></returns>
        Task<DataProvider> GetDataProviderByIdOrIdentifier(string dataProviderIdOrIdentifier);

        /// <summary>
        /// Get data provider by Identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Task<DataProvider> GetDataProviderByIdentifier(string identifier);
        Task<DataProvider> GetDataProviderByType(DataSet type);
        Task<List<Result<DataProvider>>> GetDataProvidersByIdOrIdentifier(List<string> dataProviderIdOrIdentifiers);
        Task<bool> UpdateHarvestInfo(int dataProviderId, HarvestInfo harvestInfo);
    }
}
