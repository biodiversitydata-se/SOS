using Microsoft.Extensions.Logging;
using SOS.Import.MongoDb.Interfaces;
using  SOS.Lib.Models.DarwinCore;

namespace SOS.Import.Repositories.Destination.Taxon
{
    /// <summary>
    /// Clam verbatim repository
    /// </summary>
    public class TaxonVerbatimRepository : VerbatimDbConfiguration<DarwinCoreTaxon, int>, Interfaces.ITaxonVerbatimRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="logger"></param>
        public TaxonVerbatimRepository(
            IImportClient importClient,
            ILogger<TaxonVerbatimRepository> logger) : base(importClient, logger)
        {
        }
    }
}
