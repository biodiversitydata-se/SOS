using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Import.Repositories.Resource.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Import.Managers
{
    public class DataProviderManager : Interfaces.IDataProviderManager
    {
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly ILogger<DataProviderManager> _logger;

        public DataProviderManager(
            IDataProviderRepository dataProviderRepository,
            ILogger<DataProviderManager> logger)
        {
            _dataProviderRepository = dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> AddDataProvider(DataProvider dataProvider)
        {
            return await _dataProviderRepository.AddOrUpdateAsync(dataProvider);
        }

        public async Task<bool> DeleteDataProvider(int id)
        {
            return await _dataProviderRepository.DeleteAsync(id);
        }

        public async Task<bool> UpdateDataProvider(int id, DataProvider dataProvider)
        {
            return await _dataProviderRepository.UpdateAsync(id, dataProvider);
        }

        public async Task<Result<string>> InitDefaultDataProviders(bool forceOverwriteIfCollectionExist)
        {
            bool collectionExists = await _dataProviderRepository.CheckIfCollectionExistsAsync();
            if (collectionExists && !forceOverwriteIfCollectionExist)
            {
                return Result.Failure<string>("The DataProvider collection already exists. Set forceOverwriteIfCollectionExist to true if you want to overwrite this collection with default data.");
            }

            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\DefaultDataProviders.json");
            var dataProviders = JsonConvert.DeserializeObject<List<DataProvider>>(await File.ReadAllTextAsync(filePath));
            await _dataProviderRepository.DeleteCollectionAsync();
            await _dataProviderRepository.AddCollectionAsync();
            await _dataProviderRepository.AddManyAsync(dataProviders);
            var returnDescription = collectionExists ? "DataProvider collection was created and initialized with default data providers. Existing data were overwritten." : "DataProvider collection was created and initialized with default data providers.";
            return Result.Success(returnDescription);
        }

        public async Task<DataProvider> GetDataProviderByIdAsync(int id)
        {
            var dataProviders = await _dataProviderRepository.GetAllAsync();
            var dataProvider = dataProviders.FirstOrDefault(provider => provider.Id == id);
            return dataProvider;
        }

        public async Task<List<DataProvider>> GetAllDataProviders()
        {
            return await _dataProviderRepository.GetAllAsync();
        }

        public async Task<DataProvider> GetDataProviderByIdOrIdentifier(string dataProviderIdOrIdentifier)
        {
            var allDataProviders = await _dataProviderRepository.GetAllAsync();
            return GetDataProviderByIdOrIdentifier(dataProviderIdOrIdentifier, allDataProviders);
        }

        private DataProvider GetDataProviderByIdOrIdentifier(string dataProviderIdOrIdentifier, List<DataProvider> allDataProviders)
        {
            if (int.TryParse(dataProviderIdOrIdentifier, out int id))
            {
                return allDataProviders.FirstOrDefault(provider => provider.Id == id);
            }

            return allDataProviders.FirstOrDefault(provider => provider.Identifier.Equals(dataProviderIdOrIdentifier, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<DataProvider> GetDataProviderByIdentifier(string identifier)
        {
            var allDataProviders = await _dataProviderRepository.GetAllAsync();
            return allDataProviders.FirstOrDefault(provider => provider.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<DataProvider> GetDataProviderByType(DataSet type)
        {
            if (type == DataSet.DwcA) throw new ArgumentException("Can't decide which data provider to return because there exists multiple data providers of DwC-A type.");
            var allDataProviders = await _dataProviderRepository.GetAllAsync();
            var dataProvider = allDataProviders.Single(provider => provider.Type == type);
            return dataProvider;
        }

        public async Task<List<Result<DataProvider>>> GetDataProvidersByIdOrIdentifier(List<string> dataProviderIdOrIdentifiers)
        {
            var parsedDataProviders = new List<Result<DataProvider>>();
            var allDataProviders = await _dataProviderRepository.GetAllAsync();
            foreach (var dataProviderIdOrIdentifier in dataProviderIdOrIdentifiers)
            {
                var dataProvider = GetDataProviderByIdOrIdentifier(dataProviderIdOrIdentifier, allDataProviders);
                if (dataProvider != null)
                {
                    parsedDataProviders.Add(Result.Success(dataProvider));
                }
                else
                {
                    parsedDataProviders.Add(Result.Failure<DataProvider>($"There is no data provider that has Id or Identifier = \"{dataProviderIdOrIdentifier}\""));
                }
            }

            return parsedDataProviders;
        }

        public async Task<bool> UpdateHarvestInfo(int dataProviderId, HarvestInfo harvestInfo)
        {
            return await _dataProviderRepository.UpdateHarvestInfo(dataProviderId, harvestInfo);
        }
    }
}