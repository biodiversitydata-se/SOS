using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using SOS.Lib.Models.Verbatim.Nors;

namespace SOS.Import.Repositories.Destination.Nors
{
    public class NorsObservationVerbatimRepository : VerbatimRepository<NorsObservationVerbatim, string>, Interfaces.INorsObservationVerbatimRepository
    {
        public NorsObservationVerbatimRepository(
            IImportClient importClient,
            ILogger<NorsObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}
