using System.Threading.Tasks;
using System.Collections.Generic;
using SOS.Batch.Import.AP.Entities;

namespace SOS.Batch.Import.AP.Repositories.Source.Interfaces
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
