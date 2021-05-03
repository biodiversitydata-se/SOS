using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    ///     Taxon list manager.
    /// </summary>
    public interface ITaxonListManager
    {
        /// <summary>
        ///     Get taxon lists.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TaxonList>> GetTaxonListsAsync();
    }
}