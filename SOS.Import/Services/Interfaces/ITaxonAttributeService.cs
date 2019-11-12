using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Import.Models.TaxonAttributeService;

namespace SOS.Import.Services.Interfaces
{
    /// <summary>
    /// Species data service
    /// </summary>
    public interface ITaxonAttributeService
    {
        /// <summary>
        /// Get Current red list period id
        /// </summary>
        /// <returns></returns>
        Task<int> GetCurrentRedlistPeriodIdAsync();

        /// <summary>
        /// Get taxon attributes
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TaxonAttributeModel>> GetTaxonAttributesAsync(IEnumerable<int> taxonIds, IEnumerable<int> factorIds, IEnumerable<int> periodIds);
    }
}
