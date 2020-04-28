using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Database.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    /// Area repository
    /// </summary>
    public class AreaRepository : ProcessBaseRepository<Area, int>, IAreaRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public AreaRepository(
            IProcessClient client, 
            ILogger<AreaRepository> logger) : base(client, false, logger)
        {
        }
        /// <inheritdoc />
        public async Task<InternalAreas> GetPagedAsync(string searchString, int skip, int take)
        {
            var builder = Builders<Area>.Filter;
            FilterDefinition<Area> filter;
            if (string.IsNullOrEmpty(searchString))
            {
                filter = builder.Empty;
            }
            else
            {
                filter = builder.Regex("Name", new MongoDB.Bson.BsonRegularExpression(".*" + searchString + "*.","i"));
            }
            var total = await this.MongoCollection.Find(filter).CountDocumentsAsync();
            var result = await this.MongoCollection.Find(filter).Skip(skip).Limit(take).ToListAsync();
            InternalAreas area = new InternalAreas();
            area.TotalCount = total;
            area.Areas = result;
            return area;
        }

        /// <inheritdoc />
        public async Task<Area> GetAreaAsync(int areaId)
        {
            return await GetAsync(areaId);
        }
    }
}
