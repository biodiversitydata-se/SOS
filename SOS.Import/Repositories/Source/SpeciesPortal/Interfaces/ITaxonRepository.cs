using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
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
