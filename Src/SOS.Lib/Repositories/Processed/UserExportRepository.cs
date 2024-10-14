using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Lib.Repositories.Processed
{
    /// <summary>
    ///    Log protected observations 
    /// </summary>
    public class UserExportRepository : MongoDbProcessedRepositoryBase<UserExport, int>, IUserExportRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public UserExportRepository(
            IProcessClient client,
            ILogger<UserExportRepository> logger
        ) : base(client, false, logger)
        {

        }
    }
}