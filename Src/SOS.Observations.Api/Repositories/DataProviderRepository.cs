using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    /// Data provider repository.
    /// </summary>
    public class DataProviderRepository : ProcessBaseRepository<DataProvider, int>, IDataProviderRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public DataProviderRepository(
            IProcessClient client,
            ILogger<ProcessBaseRepository<DataProvider, int>> logger) : base(client, false, logger)
        {
            
        }
    }
}
