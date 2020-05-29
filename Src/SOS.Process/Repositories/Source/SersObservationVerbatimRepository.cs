using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Verbatim.Sers;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class SersObservationVerbatimRepository : VerbatimBaseRepository<SersObservationVerbatim, string>,
        ISersObservationVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public SersObservationVerbatimRepository(
            IVerbatimClient client,
            ILogger<SersObservationVerbatimRepository> logger) : base(client, logger)
        {
        }
    }
}