using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Process.Entities;

namespace SOS.Process.Repositories.Source.SpeciesPortal.Interfaces
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
