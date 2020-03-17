using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Repository for retrieving processd taxa.
    /// </summary>
    public class TaxonProcessedRepository : ProcessBaseRepository<ProcessedTaxon, int>, ITaxonProcessedRepository
    {
        private new IMongoCollection<ProcessedTaxon> MongoCollection => Database.GetCollection<ProcessedTaxon>(_collectionName);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public TaxonProcessedRepository(
            IProcessClient client, 
            ILogger<TaxonProcessedRepository> logger) 
            : base(client, false, logger)
        {

        }

        /// <summary>
        /// Gets processed taxa.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ProcessedTaxon>> GetTaxaAsync()
        {
            try
            {
                const int batchSize = 200000;
                var skip = 0;
                var tmpTaxa = await GetChunkAsync(skip, batchSize);
                var taxa = new List<ProcessedTaxon>();

                while (tmpTaxa?.Any() ?? false)
                {
                    taxa.AddRange(tmpTaxa);
                    skip += tmpTaxa.Count();
                    tmpTaxa = await GetChunkAsync(skip, batchSize);
                }

                return taxa;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to get chunk of taxa");
                return null;
            }
        }


        private async Task<IEnumerable<ProcessedTaxon>> GetChunkAsync(int skip, int take)
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
