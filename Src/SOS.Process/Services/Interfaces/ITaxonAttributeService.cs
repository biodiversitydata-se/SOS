using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.TaxonAttributeService;

namespace SOS.Process.Services.Interfaces
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public interface ITaxonAttributeService
    {
        /// <summary>
        ///     Get Current red list period id
        /// </summary>
        /// <returns></returns>
        Task<int> GetCurrentRedlistPeriodIdAsync();

        /// <summary>
        ///     Get taxon attributes
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TaxonAttributeModel>> GetTaxonAttributesAsync(IEnumerable<int> taxonIds,
            IEnumerable<int> factorIds, IEnumerable<int> periodIds);
    }
}