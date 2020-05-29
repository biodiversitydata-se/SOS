using Microsoft.Extensions.Logging;
using SOS.Export.MongoDb.Interfaces;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Models.Processed.ProcessInfo;

namespace SOS.Export.Repositories
{
    /// <summary>
    ///     Base class for cosmos db repositories
    /// </summary>
    public class ProcessInfoRepository : BaseRepository<ProcessInfo, string>, IProcessInfoRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessInfoRepository(
            IExportClient client,
            ILogger<ProcessInfoRepository> logger
        ) : base(client, false, logger)
        {
        }
    }
}