using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
{
    /// <summary>
    /// Site repository interface
    /// </summary>
    public interface ISiteRepository
    {
        /// <summary>
        /// Get all sites 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SiteEntity>> GetAsync();
    }
}
