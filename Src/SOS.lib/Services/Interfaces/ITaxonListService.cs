using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.TaxonListService;

namespace SOS.Lib.Services.Interfaces
{
    /// <summary>
    ///     Interface for taxon list service
    /// </summary>
    public interface ITaxonListService
    {
        /// <summary>
        ///  Get taxon list definitions
        /// </summary>
        /// <returns></returns>
        Task<List<ConservationList>> GetDefinitionsAsync();

        /// <summary>
        /// Get taxa for the specified conservation lists.
        /// </summary>
        /// <param name="conservationListIds"></param>
        /// <returns></returns>
        Task<List<NatureConservationListTaxa>> GetTaxaAsync(IEnumerable<int> conservationListIds);
    }
}