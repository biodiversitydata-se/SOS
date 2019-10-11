using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
{
    /// <summary>
    /// Area repository interface
    /// </summary>
    public interface IAreaRepository
    {
        /// <summary>
        /// Get all areas
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AreaEntity>> GetAsync();
    }
}
