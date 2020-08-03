using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Destination.Kul.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Import.Repositories.Destination.Kul
{
    public class KulObservationVerbatimRepository : VerbatimRepository<KulObservationVerbatim, string>,
        IKulObservationVerbatimRepository
    {
        public KulObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<KulObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}