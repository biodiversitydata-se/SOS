using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Lib.Managers.Interfaces;

/// <summary>
///     Taxon manager
/// </summary>
public interface ITaxonManager
{        
    Task<TaxonTree<IBasicTaxon>> GetTaxonTreeAsync();

    /// <summary>
    /// Get taxon lists taxon ids.
    /// </summary>
    Task<Dictionary<int, (HashSet<int> Taxa, HashSet<int> WithUnderlyingTaxa)>> GetTaxonListSetByIdAsync();

    /// <summary>
    /// Asynchronously retrieves a collection of available taxon lists.
    /// </summary>
    /// <remarks>This method is intended to be called asynchronously and may involve network or database
    /// operations. Ensure that the calling context can handle asynchronous execution.</remarks>
    /// <returns>An enumerable collection of <see cref="TaxonList"/> objects representing the available taxon lists.</returns>
    Task<IEnumerable<TaxonList>> GetTaxonListsAsync();
}