using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Processed.Validation;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Base class for cosmos db repositories
    /// </summary>
    public class InadequateItemRepository : ProcessBaseRepository<InadequateItem>, Interfaces.IInadequateItemRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public InadequateItemRepository(
            IProcessClient client,
            ILogger<InadequateItemRepository> logger
        ) : base(client, logger)
        {
            
        }
    }
}
