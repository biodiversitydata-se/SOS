using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Search.Service.Factories.Interfaces
{
    /// <summary>
    /// Taxon factory.
    /// </summary>
    public interface ITaxonFactory
    {
        TaxonTree<IBasicTaxon> TaxonTree { get; }
    }
}
