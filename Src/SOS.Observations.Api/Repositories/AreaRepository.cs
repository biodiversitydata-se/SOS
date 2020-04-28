using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
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
        public async Task<PagedResult<Area>> GetAreasAsync(AreaType areaType, string nameFilter, int skip, int take)
        {
            var filters = new List<FilterDefinition<Area>>();

            filters.Add(Builders<Area>.Filter
                .Eq(f => f
                    .AreaType, areaType));

            if (!string.IsNullOrEmpty(nameFilter))
            {
                filters.Add(Builders<Area>.Filter
                    .Where(f => f
                        .Name.ToLower()
                        .Contains(nameFilter.ToLower())));
            }

            var filter = Builders<Area>.Filter.And(filters);

            var total = await MongoCollection
                .Find(filter)
                .CountDocumentsAsync();
            
            var result = await MongoCollection
                .Find(filter)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return new PagedResult<Area>
            {
                Records = result,
                Skip = skip,
                Take = take,
                TotalCount = total
            };
        }

        /// <inheritdoc />
        public async Task<Area> GetAreaAsync(int areaId)
        {
            return await GetAsync(areaId);
        }
    }
}
