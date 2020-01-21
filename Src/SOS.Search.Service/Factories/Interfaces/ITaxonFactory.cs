using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Sighting;

namespace SOS.Search.Service.Factories.Interfaces
{
    /// <summary>
    /// Taxon factory.
    /// </summary>
    public interface ITaxonFactory
    {
        /// <summary>
        /// Get basic taxa that can be used to create a taxon tree.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProcessedBasicTaxon>> GetBasicTaxaAsync();

        /// <summary>
        /// Get taxa
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProcessedTaxon>> GetTaxaAsync();
    }
}
