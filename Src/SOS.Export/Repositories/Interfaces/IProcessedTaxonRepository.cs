using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedTaxonRepository : IBaseRepository<Taxon, int>
    {
        /// <summary>
        ///     Get chunk of ProcessedBasicTaxon objects from repository.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<BasicTaxon>> GetBasicTaxonChunkAsync(int skip, int take);
    }
}