using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.VirtualHerbarium;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class VirtualHerbariumObservationVerbatimRepository :
        RepositoryBase<VirtualHerbariumObservationVerbatim, ObjectId>,
        IVirtualHerbariumObservationVerbatimRepository
    {
        public VirtualHerbariumObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<VirtualHerbariumObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}