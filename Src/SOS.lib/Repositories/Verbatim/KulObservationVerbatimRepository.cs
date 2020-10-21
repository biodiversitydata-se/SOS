using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    public class KulObservationVerbatimRepository : RepositoryBase<KulObservationVerbatim, string>,
        IKulObservationVerbatimRepository
    {
        public KulObservationVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<KulObservationVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}