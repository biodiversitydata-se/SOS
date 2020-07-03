using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Export.Repositories.Interfaces;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Export.Repositories
{
    /// <summary>
    /// </summary>
    public class ProcessedTaxonRepository : BaseRepository<ProcessedTaxon, int>, IProcessedTaxonRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedTaxonRepository(
            IProcessClient exportClient,
            ILogger<ProcessedTaxonRepository> logger) : base(exportClient, false, logger)
        {
        }

        /// <summary>
        ///     Get chunk of taxa
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
    }
}