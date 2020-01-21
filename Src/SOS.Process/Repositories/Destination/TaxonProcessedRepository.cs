using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Processed.Sighting;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    public class TaxonProcessedRepository : ProcessBaseRepository<ProcessedTaxon, int>, ITaxonProcessedRepository
    {
        private new IMongoCollection<ProcessedTaxon> MongoCollection => Database.GetCollection<ProcessedTaxon>(_collectionName);

        public TaxonProcessedRepository(
            IProcessClient client, 
            ILogger<TaxonProcessedRepository> logger) 
            : base(client, true, logger)
        {

        }
    }
}
