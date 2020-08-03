using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.DOI;
using SOS.Lib.Models.Search;
using SOS.Observations.Api.Repositories.Interfaces;

namespace SOS.Observations.Api.Repositories
{
    /// <summary>
    ///     Process information repository
    /// </summary>
    public class DOIRepository : ProcessBaseRepository<DOI, Guid>, IDOIRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="logger"></param>
        public DOIRepository(
            IProcessClient client,
            ILogger<DOIRepository> logger) : base(client, false, logger)
        {
        }

        /// <inheritdoc />
        public async Task<PagedResult<DOI>> GetDoisAsync(int skip, int take)
        {
            var filter = Builders<DOI>.Filter.Empty;

            var result = await MongoCollection
                .Find(filter)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return new PagedResult<DOI>
            {
                Records = result,
                Skip = skip,
                Take = take,
                TotalCount = result.Count
            };
        }
    }
}