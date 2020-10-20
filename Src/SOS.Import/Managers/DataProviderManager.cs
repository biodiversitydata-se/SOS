using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;
using SOS.Import.Managers.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Import.Managers
{
    public class DataProviderManager : IDataProviderManager
    {
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly ILogger<DataProviderManager> _logger;

        public DataProviderManager(
            IDataProviderRepository dataProviderRepository,
            ILogger<DataProviderManager> logger)
        {
            _dataProviderRepository =
                dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
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
            var collectionExists = await _dataProviderRepository.CheckIfCollectionExistsAsync();
            if (collectionExists && !forceOverwriteIfCollectionExist)
            {
                return Result.Failure<string>(
                    "The DataProvider collection already exists. Set forceOverwriteIfCollectionExist to true if you want to overwrite this collection with default data.");
            }

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\DefaultDataProviders.json");
            var dataProviders =
                JsonConvert.DeserializeObject<List<DataProvider>>(await File.ReadAllTextAsync(filePath));
            AddEmlMetadata(dataProviders);
            await _dataProviderRepository.DeleteCollectionAsync();
            await _dataProviderRepository.AddCollectionAsync();
            await _dataProviderRepository.AddManyAsync(dataProviders);
            var returnDescription = collectionExists
                ? "DataProvider collection was created and initialized with default data providers. Existing data were overwritten."
                : "DataProvider collection was created and initialized with default data providers.";
            return Result.Success(returnDescription);
        }

        /// <summary>
        /// Add EML data from the ~/Resources/EmlMetadata/ folder.
        /// </summary>
        /// <param name="dataProviders"></param>
        private void AddEmlMetadata(List<DataProvider> dataProviders)
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var emlDirectory = Path.Combine(assemblyPath, @"Resources\EmlMetadata\");
            var filePaths = Directory.GetFiles(emlDirectory, "*.xml");
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath)
                    .Replace(".eml","", StringComparison.OrdinalIgnoreCase)
                    .Replace(".xml","", StringComparison.OrdinalIgnoreCase);
                var dataProvider =
                    dataProviders.FirstOrDefault(m => m.Identifier.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                if (dataProvider != null)
                {
                    var bsonDoc = GetEmlBsonDocument(filePath);
                    dataProvider.EmlMetadata = bsonDoc;
                }
            }
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

        public async Task<DataProvider> GetDataProviderByIdentifier(string identifier)
        {
            var allDataProviders = await _dataProviderRepository.GetAllAsync();
            return allDataProviders.FirstOrDefault(provider =>
                provider.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<DataProvider> GetDataProviderByType(DataProviderType type)
        {
            if (type == DataProviderType.DwcA)
                throw new ArgumentException(
                    "Can't decide which data provider to return because there exists multiple data providers of DwC-A type.");
            var allDataProviders = await _dataProviderRepository.GetAllAsync();
            var dataProvider = allDataProviders.Single(provider => provider.Type == type);
            return dataProvider;
        }

        public async Task<List<Result<DataProvider>>> GetDataProvidersByIdOrIdentifier(
            List<string> dataProviderIdOrIdentifiers)
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
                    parsedDataProviders.Add(Result.Failure<DataProvider>(
                        $"There is no data provider that has Id or Identifier = \"{dataProviderIdOrIdentifier}\""));
                }
            }

            return parsedDataProviders;
        }


        /// <summary>
        /// Set EML metadata for a data provider.
        /// </summary>
        /// <param name="dataProviderId"></param>
        /// <param name="xmlDocument"></param>
        /// <returns></returns>
        public async Task<bool> SetEmlMetadataAsync(int dataProviderId, XDocument xmlDocument)
        {
            if (xmlDocument == null || xmlDocument.Root?.Name.LocalName != "eml") return false;
            string jsonStr = JsonConvert.SerializeXNode(xmlDocument);
            BsonDocument bsonDoc = BsonDocument.Parse(jsonStr);
            var dataProvider = await _dataProviderRepository.GetAsync(dataProviderId);
            dataProvider.EmlMetadata = bsonDoc;
            return await _dataProviderRepository.UpdateAsync(dataProviderId, dataProvider);
        }

        private BsonDocument GetEmlBsonDocument(string filePath)
        {
            XDocument xmlDocument = XDocument.Load(filePath);
            string jsonStr = JsonConvert.SerializeXNode(xmlDocument);
            BsonDocument bsonDoc = BsonDocument.Parse(jsonStr);
            return bsonDoc;
        }

        private DataProvider GetDataProviderByIdOrIdentifier(string dataProviderIdOrIdentifier,
            List<DataProvider> allDataProviders)
        {
            if (int.TryParse(dataProviderIdOrIdentifier, out var id))
            {
                return allDataProviders.FirstOrDefault(provider => provider.Id == id);
            }

            return allDataProviders.FirstOrDefault(provider =>
                provider.Identifier.Equals(dataProviderIdOrIdentifier, StringComparison.OrdinalIgnoreCase));
        }
    }
}