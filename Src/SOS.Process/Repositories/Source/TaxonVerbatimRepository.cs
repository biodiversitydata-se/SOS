using Microsoft.Extensions.Logging;
using  SOS.Lib.Models.DarwinCore;
using SOS.Process.Database.Interfaces;

namespace SOS.Process.Repositories.Source
{
    public class TaxonVerbatimRepository : VerbatimBaseRepository<DarwinCoreTaxon, int>, Interfaces.ITaxonVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public TaxonVerbatimRepository(IVerbatimClient client,
            ILogger<TaxonVerbatimRepository> logger) : base(client, logger)
        {

        }
    }
}
