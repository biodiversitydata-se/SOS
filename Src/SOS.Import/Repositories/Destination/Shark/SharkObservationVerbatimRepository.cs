using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.Repositories.Destination.Shark.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Repositories.Destination.Shark
{
    public class SharkObservationVerbatimRepository : VerbatimRepository<SharkObservationVerbatim, ObjectId>,
        ISharkObservationVerbatimRepository
    {
        public SharkObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<SharkObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}