using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class SharkObservationVerbatimRepository : VerbatimBaseRepository<SharkObservationVerbatim, ObjectId>, Interfaces.ISharkObservationVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public SharkObservationVerbatimRepository(
            IVerbatimClient client,
            ILogger<SharkObservationVerbatimRepository> logger) : base(client, logger)
        {

        }
    }
}
