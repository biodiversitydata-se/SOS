using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class SharkObservationVerbatimRepository : RepositoryBase<SharkObservationVerbatim, ObjectId>,
        ISharkObservationVerbatimRepository
    {
        public SharkObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<SharkObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}