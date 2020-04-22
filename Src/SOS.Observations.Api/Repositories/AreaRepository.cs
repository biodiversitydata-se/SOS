using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// Process information repository
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

        public async Task<InternalAreas> GetAllPagedAsync(int skip, int take)
        {
            var total = await this.MongoCollection.AsQueryable<Area>().CountAsync();
            var result = await this.MongoCollection.AsQueryable<Area>().Skip(skip).Take(take).ToListAsync();
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
