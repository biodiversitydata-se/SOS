using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Search.Service.Factories.Interfaces
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
