using Microsoft.Extensions.Logging;
using SOS.Lib.Models.Verbatim.FishData;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class FishDataObservationVerbatimRepository : VerbatimBaseRepository<FishDataObservationVerbatim, string>,
        IFishDataObservationVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public FishDataObservationVerbatimRepository(
            IVerbatimClient client,
            ILogger<FishDataObservationVerbatimRepository> logger) : base(client, logger)
        {
        }
    }
}