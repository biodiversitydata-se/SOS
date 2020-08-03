using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class NorsObservationVerbatimRepository : VerbatimBaseRepository<NorsObservationVerbatim, string>,
        INorsObservationVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public NorsObservationVerbatimRepository(
            IVerbatimClient client,
            ILogger<NorsObservationVerbatimRepository> logger) : base(client, logger)
        {
        }
    }
}