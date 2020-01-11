using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Import.Entities;

namespace SOS.Import.Repositories.Source.SpeciesPortal.Interfaces
{
    /// <summary>
    /// Organization repository interface.
    /// </summary>
    public interface IOrganizationRepository
    {
        /// <summary>
        /// Get all organizations.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<OrganizationEntity>> GetAsync();
    }
}
