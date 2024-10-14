using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class SharkObservationVerbatimRepository : VerbatimRepositoryBase<SharkObservationVerbatim, int>,
        ISharkObservationVerbatimRepository
    {
        public SharkObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<SharkObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}