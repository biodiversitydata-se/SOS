﻿using CSharpFunctionalExtensions;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SOS.Lib.Managers.Interfaces
{
    public interface IDataProviderManager
    {
        Task<bool> AddDataProvider(DataProvider dataProvider);
        Task<bool> DeleteDataProvider(int id);
        Task<bool> UpdateDataProvider(int id, DataProvider dataProvider);

        /// <summary>
        ///     Initialize DataProvider collection with default data providers.
        /// </summary>
        /// <param name="forceOverwriteIfCollectionExist">
        ///     If the DataProvider collection already exists, set
        ///     forceOverwriteIfCollectionExist to true if you want to overwrite this collection with default data.
        /// </param>
        /// <returns></returns>
        Task<Result<string>> InitDefaultDataProviders(bool forceOverwriteIfCollectionExist);

        Task<Result<string>> InitDefaultDataProvider(string dataProviderIdOrIdentifier);

        /// <summary>
        /// Reset eml to default
        /// </summary>
        /// <param name="datproviderIds"></param>
        /// <returns></returns>
        Task<Result<string>> InitDefaultEml(IEnumerable<int> datproviderIds);


        Task<List<DataProvider>> GetAllDataProvidersAsync();

        /// <summary>
        ///     Get data provider by Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<DataProvider> GetDataProviderByIdAsync(int id);

        /// <summary>
        ///     Get data provider by Id or Identifier.
        /// </summary>
        /// <param name="dataProviderIdOrIdentifier"></param>
        /// <returns></returns>
        Task<DataProvider> GetDataProviderByIdOrIdentifier(string dataProviderIdOrIdentifier);

        /// <summary>
        ///     Get data provider by Identifier.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        Task<DataProvider> GetDataProviderByIdentifier(string identifier);

        Task<DataProvider> GetDataProviderByType(DataProviderType type);

        Task<List<Result<DataProvider>>> GetDataProvidersByIdOrIdentifier(List<string> dataProviderIdOrIdentifiers);

        /// <summary>
        /// Get EML metadata from a data provider.
        /// </summary>
        /// <param name="dataProviderId"></param>
        /// <returns></returns>
        Task<XDocument> GetEmlMetadataAsync(int dataProviderId);

        /// <summary>
        /// Set EML metadata for a data provider.
        /// </summary>
        /// <param name="dataProviderId"></param>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        Task<bool> SetEmlMetadataAsync(int dataProviderId, XDocument xmlDocument);
    }
}