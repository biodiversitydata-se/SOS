using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class NorsObservationVerbatimRepository : RepositoryBase<NorsObservationVerbatim, string>,
        INorsObservationVerbatimRepository
    {
        public NorsObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<NorsObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}