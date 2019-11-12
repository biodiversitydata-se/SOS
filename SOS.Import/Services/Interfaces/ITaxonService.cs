using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.DarwinCore;

namespace SOS.Import.Services.Interfaces
{
    /// <summary>
    /// Interface for taxon service
    /// </summary>
    public interface ITaxonService
    {
        /// <summary>
        /// Get all taxa
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<DarwinCoreTaxon>> GetTaxaAsync();
    }
}
