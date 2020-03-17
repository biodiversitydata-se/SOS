using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessedTaxonRepository : ProcessBaseRepository<ProcessedTaxon, int>, IProcessedTaxonRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedTaxonRepository(
            IProcessClient client,
            ILogger<ProcessBaseRepository<ProcessedTaxon, int>> logger) : base(client, true, logger)
        {
        }

        /// <summary>
        /// Get chunk of taxa
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ProcessedBasicTaxon>> GetBasicTaxonChunkAsync(int skip, int take)
        {
            var res = await MongoCollection
                .Find(x => true)
                .Project(m => new ProcessedBasicTaxon
                {
                    DyntaxaTaxonId = m.DyntaxaTaxonId, 
                    Id = m.Id, 
                    ParentDyntaxaTaxonId = m.ParentDyntaxaTaxonId, 
                    SecondaryParentDyntaxaTaxonIds = m.SecondaryParentDyntaxaTaxonIds, 
                    ScientificName = m.ScientificName
                })
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }

        /// <summary>
        /// Get chunk of taxa
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public async Task<IEnumerable<ProcessedTaxon>> GetChunkAsync(int skip, int take)
        {
            var res = await MongoCollection
                .Find(x => true)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return res;
        }
    }
}
