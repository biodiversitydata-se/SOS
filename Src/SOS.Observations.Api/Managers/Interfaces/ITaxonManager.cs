using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Observations.Api.Managers.Interfaces
{
    /// <summary>
    /// Taxon factory.
    /// </summary>
    public interface ITaxonManager
    {
        /// <summary>
        /// Taxon Tree
        /// </summary>
        TaxonTree<IBasicTaxon> TaxonTree { get; }
    }
}
