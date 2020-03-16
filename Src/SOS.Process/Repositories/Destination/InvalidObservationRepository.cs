using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Models.Processed.Validation;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Invalid observation repository
    /// </summary>
    public class InvalidObservationRepository : ProcessBaseRepository<InvalidObservation, ObjectId>, Interfaces.IInvalidObservationRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public InvalidObservationRepository(
            IProcessClient client,
            ILogger<InvalidObservationRepository> logger
        ) : base(client, true, logger)
        {
            
        }
    }
}
