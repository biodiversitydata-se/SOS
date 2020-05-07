using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class MvmObservationVerbatimRepository : VerbatimBaseRepository<MvmObservationVerbatim, string>, Interfaces.IMvmObservationVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public MvmObservationVerbatimRepository(
            IVerbatimClient client,
            ILogger<MvmObservationVerbatimRepository> logger) : base(client, logger)
        {

        }
    }
}
