using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Search.Service.Repositories.Interfaces;

namespace SOS.Search.Service.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessedTaxonRepository : BaseRepository<ProcessedTaxon, int>, IProcessedTaxonRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mongoClient"></param>
        /// <param name="mongoDbConfiguration"></param>
        /// <param name="logger"></param>
        public ProcessedTaxonRepository(
            IMongoClient mongoClient, 
            IOptions<MongoDbConfiguration> mongoDbConfiguration,
            ILogger<BaseRepository<ProcessedTaxon, int>> logger) : base(mongoClient, mongoDbConfiguration, true, logger)
        {
        }

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
