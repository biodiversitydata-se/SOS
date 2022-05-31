using Dapper;
using Microsoft.Extensions.Logging;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen.Interfaces;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Repositories.Source.Artportalen
{
    public class SightingRelationRepository : BaseRepository<SightingRelationRepository>, ISightingRelationRepository
    {
        public SightingRelationRepository(
            IArtportalenDataService artportalenDataService,
            ILogger<SightingRelationRepository> logger) : base(artportalenDataService, logger)
        {
        }

        public async Task<IEnumerable<SightingRelationEntity>> GetAsync(IEnumerable<int> sightingIds)
        {
            try
            {
                const string query = @"
                SELECT	                
	                sr.SightingId,
	                sr.UserId,
	                sr.SightingRelationTypeId,
                    sr.Discover,
	                sr.Sort,
	                sr.IsPublic,	                
	                sr.DeterminationYear
                FROM
	                [SightingRelation] sr
                    INNER JOIN @tvp t ON sr.SightingId = t.Id AND sr.IsPublic = 1";

                return await QueryAsync<SightingRelationEntity>(query,
                    new {tvp = sightingIds.ToSqlRecords().AsTableValuedParameter("dbo.IdValueTable")});
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error getting sighting relations");
                return null!;
            }
        }
    }
}