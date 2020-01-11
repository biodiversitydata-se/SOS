using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.Validation;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class InadequateItemRepository : ProcessBaseRepository<InadequateItem, ObjectId>, Interfaces.IInadequateItemRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public InadequateItemRepository(
            IProcessClient client,
            ILogger<InadequateItemRepository> logger
        ) : base(client, true, logger)
        {
            
        }
    }
}
