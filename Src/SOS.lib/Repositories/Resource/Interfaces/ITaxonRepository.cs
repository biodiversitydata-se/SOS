using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Interfaces;

namespace SOS.Lib.Repositories.Resource.Interfaces
{
    /// <summary>
    ///     Repository for retrieving processd taxa.
    /// </summary>
    public interface ITaxonRepository : IRepositoryBase<Taxon, int>
    {
        /// <summary>
        ///     Get chunk of ProcessedBasicTaxon objects from repository.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<IBasicTaxon>> GetBasicTaxonChunkAsync(int skip, int take);

        /// <summary>
        ///     Get chunk of objects from repository
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<Taxon>> GetChunkAsync(int skip, int take);
    }
}