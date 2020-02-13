using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Export.Factories.Interfaces
{
    /// <summary>
    /// Taxon factory.
    /// </summary>
    public interface ITaxonFactory
    {
        /// <summary>
        /// Taxon Tree
        /// </summary>
        TaxonTree<IBasicTaxon> TaxonTree { get; }
    }
}
