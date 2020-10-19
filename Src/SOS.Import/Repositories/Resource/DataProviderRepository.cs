using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Import.Repositories.Resource.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Repositories.Resource
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