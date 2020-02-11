using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Sighting;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedTaxonRepository : IBaseRepository<ProcessedTaxon, int>
    {
        /// <summary>
        /// Get chunk of ProcessedBasicTaxon objects from repository.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<ProcessedBasicTaxon>> GetBasicTaxonChunkAsync(int skip, int take);
    }
}