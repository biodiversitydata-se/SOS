using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.INaturalist.Service;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class iNaturalistCompleteObservationVerbatimRepository :
        VerbatimRepositoryBase<iNaturalistVerbatimObservation, int>,
        IiNaturalistCompleteObservationVerbatimRepository
    {
        public iNaturalistCompleteObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<iNaturalistCompleteObservationVerbatimRepository> logger) : base(importClient, "iNaturalist_full", logger)
        {
        }
    }
}