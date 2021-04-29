using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class SersObservationVerbatimRepository : VerbatimRepositoryBase<SersObservationVerbatim, int>,
        ISersObservationVerbatimRepository
    {
        public SersObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<SersObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}