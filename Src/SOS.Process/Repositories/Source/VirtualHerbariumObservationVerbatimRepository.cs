using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class VirtualHerbariumObservationVerbatimRepository : VerbatimBaseRepository<VirtualHerbariumObservationVerbatim, ObjectId>, Interfaces.IVirtualHerbariumObservationVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public VirtualHerbariumObservationVerbatimRepository(
            IVerbatimClient client,
            ILogger<VirtualHerbariumObservationVerbatimRepository> logger) : base(client, logger)
        {

        }
    }
}
