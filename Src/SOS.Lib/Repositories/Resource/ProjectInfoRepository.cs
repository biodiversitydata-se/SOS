using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Resource
{
    /// <summary>
    ///     Project repository.
    /// </summary>
    public class ProjectInfoRepository : RepositoryBase<ProjectInfo, int>, IProjectInfoRepository
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="processClient"></param>
        /// <param name="logger"></param>
        public ProjectInfoRepository(
            IProcessClient processClient,
            ILogger<ProjectInfoRepository> logger) : base(processClient, logger)
        {
        }

        /// <inheritdoc/>
        public async Task CreateIndexesAsync()
        {
            await base.AddIndexes([
                new CreateIndexModel<ProjectInfo>(Builders<ProjectInfo>.IndexKeys
                    .Text(pi => pi.Category)
                    .Text(pi => pi.CategorySwedish)
                    .Text(pi => pi.Name)
                ),
                new CreateIndexModel<ProjectInfo>(Builders<ProjectInfo>.IndexKeys.Ascending(pi => pi.ControlingOrganisationId)),
                new CreateIndexModel<ProjectInfo>(Builders<ProjectInfo>.IndexKeys.Ascending(pi => pi.IsPublic)),
                new CreateIndexModel<ProjectInfo>(Builders<ProjectInfo>.IndexKeys.Ascending(pi => pi.UserServiceUserId))
           ]);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProjectInfo>> GetAsync(string filter, int userId)
        {
            var filters = new[] {
                Builders<ProjectInfo>.Filter.Or(
                    Builders<ProjectInfo>.Filter.Eq(p => p.IsPublic, true),
                    Builders<ProjectInfo>.Filter.Eq(p => p.UserServiceUserId, userId),
                    Builders<ProjectInfo>.Filter.Eq(p => p.MemberIds.Contains(userId), true)
                ),
                Builders<ProjectInfo>.Filter.Text(filter, new TextSearchOptions{ CaseSensitive = false })
            };

            var res = await MongoCollection.FindAsync(Builders<ProjectInfo>.Filter.And(filters)).Result.ToListAsync();
            return res;
        }
    }
}