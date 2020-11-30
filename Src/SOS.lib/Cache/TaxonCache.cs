using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Lib.Cache
{
    /// <summary>
    /// Taxon cache
    /// </summary>
    public class TaxonCache : CacheBase<int, Taxon>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonRepository"></param>
        public TaxonCache(ITaxonRepository taxonRepository) : base(taxonRepository)
        {

        }
    }
}