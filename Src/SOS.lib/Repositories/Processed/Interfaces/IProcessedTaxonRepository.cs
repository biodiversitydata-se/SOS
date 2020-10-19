using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Repositories.Processed.Interfaces
{
    /// <summary>
    ///     Repository for retrieving processd taxa.
    /// </summary>
    public interface IProcessedTaxonRepository : IMongoDbProcessedRepositoryBase<Taxon, int>
    {
        /// <summary>
        ///     Get chunk of ProcessedBasicTaxon objects from repository.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<BasicTaxon>> GetBasicTaxonChunkAsync(int skip, int take);

        /// <summary>
        ///     Get chunk of objects from repository
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<Taxon>> GetChunkAsync(int skip, int take);
    }
}