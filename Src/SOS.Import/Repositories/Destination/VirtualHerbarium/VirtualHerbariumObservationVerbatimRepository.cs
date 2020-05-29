using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Import.MongoDb.Interfaces;
using SOS.Import.Repositories.Destination.VirtualHerbarium.Interfaces;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;

namespace SOS.Import.Repositories.Destination.VirtualHerbarium
{
    public class VirtualHerbariumObservationVerbatimRepository :
        VerbatimRepository<VirtualHerbariumObservationVerbatim, ObjectId>,
        IVirtualHerbariumObservationVerbatimRepository
    {
        public VirtualHerbariumObservationVerbatimRepository(
            IImportClient importClient,
            ILogger<VirtualHerbariumObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}