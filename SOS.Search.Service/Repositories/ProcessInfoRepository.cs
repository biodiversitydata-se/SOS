using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// Process information repository
    /// </summary>
    public class ProcessInfoRepository : BaseRepository<ProcessInfo, byte>, IProcessInfoRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessInfoRepository(
            IMongoClient mongoClient,
            IOptions<MongoDbConfiguration> mongoDbConfiguration, 
            ILogger<ProcessInfoRepository> logger) : base(mongoClient, mongoDbConfiguration, false, logger)
        {
        }

        /// <inheritdoc />
        public async Task<ProcessInfo> GetProcessInfoAsync(bool current)
        {
            var instance = (byte)(current ? ActiveInstance : ActiveInstance == 0 ? 1 : 0);
            return await GetAsync(instance);
        }
    }
}
