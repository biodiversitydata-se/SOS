using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Models.Processed;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination.Interfaces;

namespace SOS.Process.Repositories.Destination
{
    /// <summary>
    /// Repository for retrieving processed areas.
    /// </summary>
    public class ProcessedAreaRepository : ProcessBaseRepository<Area, int>, IProcessedAreaRepository
    {
        private new IMongoCollection<Area> MongoCollection => Database.GetCollection<Area>(_collectionName);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public ProcessedAreaRepository(
            IProcessClient client, 
            ILogger<ProcessedAreaRepository> logger) 
            : base(client, false, logger)
        {

        }

        /// <inheritdoc />
        public async Task<List<AreaBase>> GetAllAreaBaseAsync()
        {
            var res = await MongoCollection
                .Find(x => true)
                .Project(m => new AreaBase(m.AreaType)
                {
                    FeatureId = m.FeatureId,
                    Id = m.Id,
                    Name = m.Name,
                    ParentId = m.ParentId,
                })
                .ToListAsync();

            return res;
        }
    }
}