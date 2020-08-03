using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.ClamPortal;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class ClamObservationVerbatimRepository : VerbatimBaseRepository<ClamObservationVerbatim, ObjectId>,
        IClamObservationVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ClamObservationVerbatimRepository(IVerbatimClient client,
            ILogger<ClamObservationVerbatimRepository> logger) : base(client, logger)
        {
        }
    }
}