using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Repositories.Resource
{
    public class DataProviderRepository : ResourceRepositoryBase<DataProvider, int>, IDataProviderRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public DataProviderRepository(
            IProcessClient processClient,
            ILogger<DataProviderRepository> logger) : base(processClient, false, logger)
        {
        }

        public override async Task<List<DataProvider>> GetAllAsync()
        {
            var allDataProviders = await base.GetAllAsync();
            return allDataProviders.OrderBy(provider => provider.Id).ToList();
        }
    }
}