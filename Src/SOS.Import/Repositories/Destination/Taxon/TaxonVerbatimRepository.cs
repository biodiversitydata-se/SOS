using Microsoft.Extensions.Logging;
using SOS.Import.Repositories.Destination.Taxon.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Import.Repositories.Destination.Taxon
{
    /// <summary>
    ///     Clam verbatim repository
    /// </summary>
    public class TaxonVerbatimRepository : VerbatimRepository<DarwinCoreTaxon, int>, ITaxonVerbatimRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public TaxonVerbatimRepository(
            IVerbatimClient importClient,
            ILogger<TaxonVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}