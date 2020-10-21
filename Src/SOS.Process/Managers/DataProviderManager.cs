using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Process.Managers.Interfaces;

namespace SOS.Process.Managers
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

        public async Task<DataProvider> GetDataProviderByIdAsync(int id)
        {
            var dataProviders = await _dataProviderRepository.GetAllAsync();
            var dataProvider = dataProviders.FirstOrDefault(provider => provider.Id == id);
            return dataProvider;
        }

        public async Task<List<DataProvider>> GetAllDataProvidersAsync()
        {
            return await _dataProviderRepository.GetAllAsync();
        }
    }
}