using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Batch.Import.AP.Entities;

namespace SOS.Batch.Import.AP.Repositories.Source.Interfaces
{
    /// <summary>
    /// Taxon repository interface
    /// </summary>
    public interface ITaxonRepository
    {
        /// <summary>
        /// Get all taxa 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TaxonEntity>> GetAsync();
    }
}
