using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Models.Verbatim.ClamTreePortal;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class ClamObservationVerbatimRepository : VerbatimBaseRepository<ClamObservationVerbatim, ObjectId>, Interfaces.IClamObservationVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ClamObservationVerbatimRepository(IVerbatimClient client,
            ILogger<ClamObservationVerbatimRepository> logger) : base(client, logger)
        {

        }
    }
}
