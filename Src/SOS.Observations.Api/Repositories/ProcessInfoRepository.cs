using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    /// Process information repository
    /// </summary>
    public class ProcessInfoRepository : ProcessBaseRepository<ProcessInfo, byte>, IProcessInfoRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessInfoRepository(
            IProcessClient client, 
            ILogger<ProcessInfoRepository> logger) : base(client, false, logger)
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
