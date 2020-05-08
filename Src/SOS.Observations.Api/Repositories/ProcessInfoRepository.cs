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
    public class ProcessInfoRepository : ProcessBaseRepository<ProcessInfo, string>, IProcessInfoRepository
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
            return await GetAsync($"ProcessedObservation-{ (current ? ActiveInstance : ActiveInstance == 1 ? 0 : 1) }");
        }
    }
}
