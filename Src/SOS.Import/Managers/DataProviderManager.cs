using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Managers
{
    public class DataProviderManager : Interfaces.IDataProviderManager
    {
        private readonly IDataProviderRepostitory _dataProviderRepository;
        private readonly ILogger<DataProviderManager> _logger;

        public DataProviderManager(
            IDataProviderRepostitory dataProviderRepository,
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

        public async Task<bool> InitDefaultDataProviders()
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath, @"Resources\DefaultDataProviders.json");
            var dataProviders = JsonConvert.DeserializeObject<List<DataProvider>>(await File.ReadAllTextAsync(filePath));
            await _dataProviderRepository.DeleteCollectionAsync();
            await _dataProviderRepository.AddCollectionAsync();
            return await _dataProviderRepository.AddManyAsync(dataProviders);
        }

        public async Task<DataProvider> TryGetDataProviderAsync(int id)
        {
            var dataProviders = await _dataProviderRepository.GetAllAsync();
            var dataProvider = dataProviders.FirstOrDefault(provider => provider.Id == id);
            return dataProvider;
        }
    }
}
