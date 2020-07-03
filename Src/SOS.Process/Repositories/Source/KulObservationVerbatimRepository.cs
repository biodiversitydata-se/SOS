using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Kul;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class KulObservationVerbatimRepository : VerbatimBaseRepository<KulObservationVerbatim, string>,
        IKulObservationVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public KulObservationVerbatimRepository(
            IVerbatimClient client,
            ILogger<KulObservationVerbatimRepository> logger) : base(client, logger)
        {
        }
    }
}