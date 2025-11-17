using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public Task<Dictionary<int, (HashSet<int> Taxa, HashSet<int> WithUnderlyingTaxa)>> GetTaxonListSetByIdAsync();
}