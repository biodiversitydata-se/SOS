using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Destination.Nors.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Nors;

namespace SOS.Import.Repositories.Destination.Nors
{
    public class NorsObservationVerbatimRepository : VerbatimRepository<NorsObservationVerbatim, string>,
        INorsObservationVerbatimRepository
    {
        public NorsObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<NorsObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}