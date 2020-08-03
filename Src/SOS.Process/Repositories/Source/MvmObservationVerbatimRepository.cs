using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class MvmObservationVerbatimRepository : VerbatimBaseRepository<MvmObservationVerbatim, string>,
        IMvmObservationVerbatimRepository
    {
        /// <summary>
        ///     Constructor
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