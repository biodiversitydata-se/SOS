using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Sighting;

namespace SOS.Search.Service.Repositories.Interfaces
{
    public interface IProcessedTaxonRepository : IBaseRepository<ProcessedTaxon, int>
    {
        /// <summary>
        /// Get chunk of objects from repository
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<ProcessedTaxon>> GetChunkAsync(int skip, int take);

        /// <summary>
        /// Get chunk of ProcessedBasicTaxon objects from repository.
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<IEnumerable<ProcessedBasicTaxon>> GetBasicTaxonChunkAsync(int skip, int take);
    }
}