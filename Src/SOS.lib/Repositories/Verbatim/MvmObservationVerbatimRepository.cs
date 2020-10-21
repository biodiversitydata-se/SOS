using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class MvmObservationVerbatimRepository : RepositoryBase<MvmObservationVerbatim, string>,
        IMvmObservationVerbatimRepository
    {
        public MvmObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<MvmObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}