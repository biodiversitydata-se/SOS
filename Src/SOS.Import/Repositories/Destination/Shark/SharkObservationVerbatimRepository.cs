using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Repositories.Destination.Shark
{
    public class SharkObservationVerbatimRepository : VerbatimRepository<SharkObservationVerbatim, ObjectId>, Interfaces.ISharkObservationVerbatimRepository
    {
        public SharkObservationVerbatimRepository(
            IImportClient importClient,
            ILogger<SharkObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}
