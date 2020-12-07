using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    ///     Taxon manager
    /// </summary>
    public interface ITaxonManager
    {
        /// <summary>
        ///     Taxon Tree
        /// </summary>
        TaxonTree<IBasicTaxon> TaxonTree { get; }
    }
}